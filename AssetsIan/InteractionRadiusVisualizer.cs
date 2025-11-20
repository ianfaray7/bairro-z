using UnityEngine;

/// <summary>
/// Componente visual para mostrar o raio de interação dos tiles
/// Adicione este script aos BuildTiles para ver o raio no jogo
/// </summary>
public class InteractionRadiusVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private bool showInGame = true;
    [SerializeField] private Color idleColor = new Color(1, 1, 0, 0.2f); // Amarelo transparente
    [SerializeField] private Color activeColor = new Color(0, 1, 0, 0.4f); // Verde transparente
    [SerializeField] private int segments = 50;
    
    private LineRenderer lineRenderer;
    private BuildTile buildTile;
    
    void Awake()
    {
        // Cria LineRenderer automaticamente
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        buildTile = GetComponent<BuildTile>();
        
        SetupLineRenderer();
    }
    
    void SetupLineRenderer()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = idleColor;
        lineRenderer.endColor = idleColor;
        lineRenderer.sortingOrder = -1;
        
        // Desabilita se não deve mostrar no jogo
        lineRenderer.enabled = showInGame;
    }
    
    void Update()
    {
        if (!showInGame)
            return;
            
        DrawCircle();
    }
    
    void DrawCircle()
    {
        float radius = 2f; // Valor padrão
        
        // Tenta pegar o raio do BuildTile via reflection
        if (buildTile != null)
        {
            var field = buildTile.GetType().GetField("interactionRadius", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                radius = (float)field.GetValue(buildTile);
            }
        }
        
        float deltaTheta = (2f * Mathf.PI) / segments;
        float theta = 0f;
        
        for (int i = 0; i <= segments; i++)
        {
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            theta += deltaTheta;
        }
    }
    
    /// <summary>
    /// Muda a cor do círculo (pode ser chamado quando jogador está perto)
    /// </summary>
    public void SetActive(bool active)
    {
        Color targetColor = active ? activeColor : idleColor;
        lineRenderer.startColor = targetColor;
        lineRenderer.endColor = targetColor;
    }
}
