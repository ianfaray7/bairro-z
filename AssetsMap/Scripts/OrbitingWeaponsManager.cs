using UnityEngine;
using System.Collections.Generic;

public class OrbitingWeaponsManager : MonoBehaviour
{
    [Header("Config")]
    public GameObject weaponPrefab;     // prefab do "weapon" (apenas um objeto com Sprite e Weapon script)
    public int weaponCount = 3;
    public float radius = 1.2f;
    public float orbitSpeed = 60f; // graus por seg (se quiser girar continuamente)
    public bool orbitContinuously = false; // se as armas giram por conta própria

    [Header("Shooting")]
    public KeyCode fireKey = KeyCode.Mouse0; // clique esquerdo
    public float globalFireCooldown = 0.12f; // entre disparos por arma
    float fireTimer = 0f;

    List<Transform> weapons = new List<Transform>();
    PlayerController player;

    void Awake()
    {
        player = GetComponent<PlayerController>();
        if (player == null)
            Debug.LogError("OrbitingWeaponsManager precisa do PlayerController no mesmo GameObject.");
    }

    void Start()
    {
        SpawnWeapons();
    }

    void SpawnWeapons()
    {
        // destrói antigas (se houver)
        foreach (Transform t in weapons) if (t != null) Destroy(t.gameObject);
        weapons.Clear();

        // cria weapons equally spaced
        for (int i = 0; i < weaponCount; i++)
        {
            GameObject w = Instantiate(weaponPrefab, transform);
            w.name = "Weapon_" + i;
            weapons.Add(w.transform);
        }
        LayoutWeaponsInstant();
    }

    void Update()
    {
        // opcional: rotacionar globalmente as armas
        if (orbitContinuously)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                // faz girar o ângulo base com base no tempo
                float angle = (360f / weaponCount) * i + Time.time * orbitSpeed;
                Vector2 pos = AngleToPosition(angle, radius);
                weapons[i].localPosition = pos;
            }
        }
        else
        {
            LayoutWeaponsInstant();
        }

        // rotaciona each weapon para mirar no mouse
        for (int i = 0; i < weapons.Count; i++)
        {
            Transform wt = weapons[i];
            Vector2 dir = (Vector2)player.MouseWorldPosition - (Vector2)wt.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            wt.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        fireTimer -= Time.deltaTime;
        if ((Input.GetMouseButton(0) || Input.GetKey(fireKey)) && fireTimer <= 0f)
        {
            FireAllWeapons();
            fireTimer = globalFireCooldown;
        }

        // hotkeys para testar: aumentar/diminuir armas
        if (Input.GetKeyDown(KeyCode.E)) { weaponCount++; SpawnWeapons(); }
        if (Input.GetKeyDown(KeyCode.Q) && weaponCount > 0) { weaponCount--; SpawnWeapons(); }
    }

    void LayoutWeaponsInstant()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            float baseAngle = (360f / weapons.Count) * i;
            Vector2 pos = AngleToPosition(baseAngle, radius);
            weapons[i].localPosition = pos;
        }
    }

    Vector2 AngleToPosition(float angleDeg, float r)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * r;
    }

    void FireAllWeapons()
    {
        foreach (Transform wt in weapons)
        {
            Weapon weaponScript = wt.GetComponent<Weapon>();
            if (weaponScript != null)
            {
                weaponScript.Fire(player.MouseWorldPosition);
            }
        }
    }
}
