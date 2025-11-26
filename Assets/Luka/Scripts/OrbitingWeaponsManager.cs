using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OrbitingWeaponsManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject weaponPrefab;      // Weapon prefab (contains Weapon.cs)
    public int weaponCount = 2;

    [Header("Layout")]
    public float radius = 0.9f;          // distância do centro do player
    public bool smoothPositions = true;
    public float smoothSpeed = 10f;

    [Header("Fire")]
    public bool holdToFire = true;       // true = atira segurando mouse, false = tiro por clique
    public float globalCooldown = 0.12f; // tempo mínimo entre rajadas completas
    float fireTimer = 0f;

    [Header("Flip / Orientação")]
    public bool mirrorWithPlayerFlip = true; // se true, espelha localPosition.x quando o player flipX = true
    public bool flipWeaponSpriteWithPlayer = true; // se true, aplica wr.flipX = playerSprite.flipX
    [Tooltip("Ajuste se o sprite da arma estiver 90° fora (ex.: set 90 ou -90)")]
    public float rotationOffset = -90f; // se o sprite da arma "aponta" para cima, coloque -90 ou 90 pra corrigir

    List<Transform> weapons = new List<Transform>();
    Camera mainCam;
    SpriteRenderer playerSprite;
    Vector2 lastAimDirection = Vector2.right; // Guarda última direção de mira

    void Awake()
    {
        mainCam = Camera.main;
        playerSprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SpawnWeapons();
    }

    void Update()
    {
        if (weaponPrefab == null) return;

        if (PauseManager.IsPaused) return; // don't process weapons when paused
        UpdateWeaponsPositionAndRotation();

        fireTimer -= Time.deltaTime;
        
        // Suporte para controles mobile e mouse
        bool fireInput = GetFireInput();

        if (fireInput && fireTimer <= 0f)
        {
            FireAllWeapons();
            fireTimer = globalCooldown;
        }

        // hotkeys rápidos para debug
        if (Input.GetKeyDown(KeyCode.E)) { weaponCount++; SpawnWeapons(); }
        if (Input.GetKeyDown(KeyCode.Q) && weaponCount > 0) { weaponCount--; SpawnWeapons(); }
    }

    void SpawnWeapons()
    {
        // destrói instâncias antigas
        foreach (Transform t in weapons) if (t != null) Destroy(t.gameObject);
        weapons.Clear();

        // cria weapons como filhos deste objeto (mantêm posição relativa ao jogador)
        for (int i = 0; i < weaponCount; i++)
        {
            GameObject w = Instantiate(weaponPrefab, transform);
            w.name = "Weapon_" + i;
            weapons.Add(w.transform);
        }

        LayoutWeaponsInstant();
    }

    void LayoutWeaponsInstant()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            float ang = (360f / Mathf.Max(1, weapons.Count)) * i;
            Vector2 pos = AngleToPosition(ang, radius);
            // se mirrorWithPlayerFlip está ativado, já aplica o espelho conforme o estado atual do player
            if (mirrorWithPlayerFlip && playerSprite != null && playerSprite.flipX)
                pos.x = -Mathf.Abs(pos.x);
            weapons[i].localPosition = pos;
        }
    }

    void UpdateWeaponsPositionAndRotation()
    {
        Vector2 aimDirection = GetAimDirection();
        // Cria um ponto de mira a uma distância fixa para rotação das armas
        Vector2 aimTarget = (Vector2)transform.position + (aimDirection * 5f);

        for (int i = 0; i < weapons.Count; i++)
        {
            Transform wt = weapons[i];

            float angBase = (360f / Mathf.Max(1, weapons.Count)) * i;
            Vector2 targetLocal = AngleToPosition(angBase, radius);

            // Espelhar a posição X se o player estiver flipado e a opção estiver ativa
            if (mirrorWithPlayerFlip && playerSprite != null && playerSprite.flipX)
            {
                targetLocal.x = -Mathf.Abs(targetLocal.x);
            }
            else
            {
                targetLocal.x = Mathf.Abs(targetLocal.x);
            }

            if (smoothPositions)
                wt.localPosition = Vector2.Lerp(wt.localPosition, targetLocal, Time.deltaTime * smoothSpeed);
            else
                wt.localPosition = targetLocal;

            // Rotação: arma mira para o ponto de mira
            Vector2 dir = aimTarget - (Vector2)wt.position;
            if (dir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // aplica offset de rotação (p.ex. +90/-90 para sprites que apontam para cima)
                angle += rotationOffset;

                wt.rotation = Quaternion.Euler(0, 0, angle-90);
            }

            // Opcional: flipa visualmente o sprite da arma conforme o flip do player
            if (flipWeaponSpriteWithPlayer)
            {
                SpriteRenderer wr = wt.GetComponent<SpriteRenderer>();
                if (wr != null && playerSprite != null)
                {
                    wr.flipX = playerSprite.flipX;
                }
            }
        }
    }

    Vector2 AngleToPosition(float angleDeg, float r)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * r;
    }

    void FireAllWeapons()
    {
        // Pega a direção de mira (joystick ou mouse)
        Vector2 aimDir = GetAimDirection();
        
        foreach (Transform wt in weapons)
        {
            Weapon w = wt.GetComponent<Weapon>();
            if (w != null) w.Fire(aimDir); // Passa a direção para o Weapon
        }
    }
    
    /// <summary>
    /// Obtém input de disparo (mouse ou joystick mobile)
    /// </summary>
    private bool GetFireInput()
    {
        // Em mobile, APENAS o joystick de ataque pode atirar
#if UNITY_ANDROID || UNITY_IOS
        if (AttackJoystick.Instance != null && AttackJoystick.Instance.IsAttacking())
        {
            return true;
        }
        return false;
#else
        // Em PC/Web, tenta joystick primeiro, depois mouse
        if (AttackJoystick.Instance != null && AttackJoystick.Instance.IsAttacking())
        {
            return true;
        }
        
        // Fallback para mouse
        return holdToFire ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
#endif
    }
    
    /// <summary>
    /// Obtém direção de mira (joystick mobile ou mouse)
    /// </summary>
    private Vector2 GetAimDirection()
    {
        // MOBILE: Usa apenas o joystick
#if UNITY_ANDROID || UNITY_IOS
        if (AttackJoystick.Instance != null)
        {
            Vector2 joyInput = AttackJoystick.Instance.GetInput();
            if (joyInput.sqrMagnitude > 0.01f)
            {
                return joyInput.normalized;
            }
        }
        // Se não tem input, mantém última direção
        return lastAimDirection;
        
        // PC/WEB: Usa joystick se disponível, senão mouse
#else
        if (AttackJoystick.Instance != null)
        {
            Vector2 joyInput = AttackJoystick.Instance.GetInput();
            if (joyInput.sqrMagnitude > 0.01f)
            {
                lastAimDirection = joyInput.normalized;
                return lastAimDirection;
            }
        }
        
        // Mouse: Calcula direção do player para o mouse
        if (mainCam != null)
        {
            Vector3 mp = Input.mousePosition;
            Vector2 mouseWorld = mainCam.ScreenToWorldPoint(mp);
            Vector2 dirToMouse = mouseWorld - (Vector2)transform.position;
            if (dirToMouse.sqrMagnitude > 0.01f)
            {
                lastAimDirection = dirToMouse.normalized;
            }
        }
        
        return lastAimDirection;
#endif
    }
}

