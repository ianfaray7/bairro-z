using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class MapEdgeWalls : MonoBehaviour
{
    [Header("Source Bounds")]
    public Tilemap sampleTilemap; // se definido, usa os bounds do tilemap
    public BoxCollider2D sampleBoundsCollider; // ou, se definido, usa o collider
    [Tooltip("Se true, calcula as bounds a partir de todos os Renderers/Colliders filhos (útil quando seu mapa é composto por múltiplos GameObjects)")]
    public bool useChildrenBounds = true;
    [Header("Inset / padding")]
    [Tooltip("Diminuir a extensão vertical das bordas; por exemplo 1.46 fará top/bottom ir de +/-8.16 para +/-6.7")] 
    public float verticalInset = 0f;
    [Tooltip("Diminuir a extensão horizontal das bordas (opcional).")]
    public float horizontalInset = 0f;
    public Vector2 manualCenter; // usado se nenhum fonte definido
    // If your maps are 900x600 (in units used by the level), you can set this as the fallback.
    // Note: Unity world units may differ from pixel size — use Tilemap bounds or BoxCollider2D when possible.
    public Vector2 manualSize = new Vector2(900f, 600f);

    [Header("Walls")]
    public float thickness = 1f;
    [Tooltip("Se true, adiciona um BoxCollider2D ao GameObject do mapa com as dimensões detectadas — útil para debug e reutilização das bounds")]
    public bool addDebugBoundsCollider = false;
    public bool createOnAwake = true;
    public string playerLayerName = "Player"; // nome da layer do jogador
    public string wallLayerName = "PlayerWall"; // nome da layer para as paredes de borda
    public bool autoDetectPlayerAndSetLayer = true;
    [Tooltip("Se true e o jogador não tiver collider, adiciona automaticamente um CircleCollider2D (editor runtime).")]
    public bool autoAddColliderToPlayer = false;

    [Header("Editor/Runtime Helpers")]
    public bool autoSetupLayerCollisions = true; // aplica Physics2D.IgnoreLayerCollision para que só o player colida

    // children reused if existing
    List<GameObject> walls = new List<GameObject>();

    void Awake()
    {
        if (createOnAwake && Application.isPlaying) CreateOrUpdateWalls();
    }

    void OnValidate()
    {
        if (!Application.isPlaying && autoSetupLayerCollisions) SetupLayerCollision();
    }

    public void CreateOrUpdateWalls()
    {
        Bounds bounds = GetMapBounds();
        Debug.Log($"MapEdgeWalls: CreateOrUpdateWalls bounds center={bounds.center} size={bounds.size}");
        CreateWallsFromBounds(bounds);
        if (autoDetectPlayerAndSetLayer) TrySetPlayerLayer();
        if (autoSetupLayerCollisions) SetupLayerCollision();
    }

    Bounds GetMapBounds()
    {
        // 1. Tilemap
        if (sampleTilemap != null)
        {
            // localBounds já dá o tamanho do tilemap
            var lb = sampleTilemap.localBounds;
            Vector3 minWorld = sampleTilemap.transform.TransformPoint(lb.min);
            Vector3 maxWorld = sampleTilemap.transform.TransformPoint(lb.max);
            var b = new Bounds();
            b.SetMinMax(minWorld, maxWorld);
            return b;
        }
        if (sampleBoundsCollider != null)
        {
            return sampleBoundsCollider.bounds;
        }
        // 3. children renderers/colliders union
        if (useChildrenBounds)
        {
            Bounds? total = null;
            // renderers
            var rends = GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                if (r.gameObject == gameObject) continue;
                if (r.name.StartsWith("EdgeWall_")) continue; // ignore walls we create
                var b = r.bounds;
                if (!total.HasValue) total = b;
                else { var t = total.Value; t.Encapsulate(b); total = t; }
            }
            // colliders
            var cols = GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols)
            {
                if (c.gameObject == gameObject) continue;
                if (c.name.StartsWith("EdgeWall_")) continue; // ignore walls we create
                var b = c.bounds;
                if (!total.HasValue) total = b;
                else { var t = total.Value; t.Encapsulate(b); total = t; }
            }
            if (total.HasValue)
            {
                return total.Value;
            }
        }
        // fallback: manual or camera
        if (manualSize.x > 0 && manualSize.y > 0)
        {
            return new Bounds((Vector3)manualCenter, new Vector3(manualSize.x, manualSize.y, 1f));
        }
        // fallback camera
        if (Camera.main != null && Camera.main.orthographic)
        {
            var cam = Camera.main;
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            var center = cam.transform.position;
            return new Bounds(center, new Vector3(width, height, 1f));
        }
        // default
        return new Bounds(Vector3.zero, new Vector3(900f, 600f, 1f));
    }

    void CreateWallsFromBounds(Bounds bounds)
    {
        // Remove older walls if any
        foreach (var w in walls)
        {
            if (w != null) DestroyImmediate(w);
        }
        walls.Clear();

        // optionally attach a debug collider to the root object for visualization
        if (addDebugBoundsCollider)
        {
            var bc = GetComponent<BoxCollider2D>();
            if (bc == null) bc = gameObject.AddComponent<BoxCollider2D>();
            bc.offset = transform.InverseTransformPoint(bounds.center);
            bc.size = new Vector2(bounds.size.x, bounds.size.y);
            bc.isTrigger = true;
        }
        // extents
        float halfW = bounds.size.x * 0.5f - horizontalInset;
        float halfH = bounds.size.y * 0.5f - verticalInset;
        // clamp to avoid negative sizes
        halfW = Mathf.Max(0.01f, halfW);
        halfH = Mathf.Max(0.01f, halfH);
        float x = bounds.center.x;
        float y = bounds.center.y;

        // top
        var top = NewWall("EdgeWall_Top");
        top.transform.position = new Vector3(x, y + halfH + thickness * 0.5f, 0f);
        top.GetComponent<BoxCollider2D>().size = new Vector2(bounds.size.x + thickness * 2f, thickness);
        walls.Add(top);

        // bottom
        var bottom = NewWall("EdgeWall_Bottom");
        bottom.transform.position = new Vector3(x, y - halfH - thickness * 0.5f, 0f);
        bottom.GetComponent<BoxCollider2D>().size = new Vector2(bounds.size.x + thickness * 2f, thickness);
        walls.Add(bottom);

        // left
        var left = NewWall("EdgeWall_Left");
        left.transform.position = new Vector3(x - halfW - thickness * 0.5f, y, 0f);
        left.GetComponent<BoxCollider2D>().size = new Vector2(thickness, bounds.size.y + thickness * 2f);
        walls.Add(left);

        // right
        var right = NewWall("EdgeWall_Right");
        right.transform.position = new Vector3(x + halfW + thickness * 0.5f, y, 0f);
        right.GetComponent<BoxCollider2D>().size = new Vector2(thickness, bounds.size.y + thickness * 2f);
        walls.Add(right);
    }

    GameObject NewWall(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, true);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        // set layer
        int wl = LayerMask.NameToLayer(wallLayerName);
        if (wl >= 0) go.layer = wl;
        return go;
    }

    void TrySetPlayerLayer()
    {
        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        if (playerLayer < 0)
        {
            Debug.LogWarning($"MapEdgeWalls: layer '{playerLayerName}' não existe. Por favor adicione a layer nas Project Settings > Tags & Layers.");
            return;
        }
        // detect PlayerController e atribui a layer
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            if (player.gameObject.layer != playerLayer)
            {
                Debug.Log($"MapEdgeWalls: definindo layer '{playerLayerName}' para Player {player.gameObject.name}.");
                player.gameObject.layer = playerLayer;
            }

            if (autoAddColliderToPlayer)
            {
                var col = player.GetComponent<Collider2D>();
                if (col == null)
                {
                    var circle = player.gameObject.AddComponent<CircleCollider2D>();
                    circle.radius = 0.5f; // default guess — ajuste no Inspector se necessário
                    Debug.Log($"MapEdgeWalls: CircleCollider2D adicionado automaticamente para {player.gameObject.name}.");
                }
            }
        }
        else
        {
            Debug.Log("MapEdgeWalls: PlayerController não encontrado para definir a layer automaticamente. Defina manualmente se necessário.");
        }
    }

    void SetupLayerCollision()
    {
        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        int wallLayer = LayerMask.NameToLayer(wallLayerName);
        if (playerLayer < 0 || wallLayer < 0)
        {
            // não finalize sem layers
            return;
        }

        for (int i = 0; i < 32; i++)
        {
            // ignore wall for all but player
            if (i == playerLayer)
            {
                Physics2D.IgnoreLayerCollision(wallLayer, i, false);
            }
            else
            {
                Physics2D.IgnoreLayerCollision(wallLayer, i, true);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Create Walls Now")]
    void ContextCreateWalls()
    {
        CreateOrUpdateWalls();
        // mark scene dirty
        if (!Application.isPlaying) EditorUtility.SetDirty(this);
    }
#endif
}
