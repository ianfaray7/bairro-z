using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform muzzlePoint;         // child pos no extremo da arma (se vazio, usa transform)
    public float projectileSpeed = 14f;
    public float localCooldown = 0f;      // cooldown por arma (se quiser variar)
    public bool playShootAnimationTrigger = true;
    public string shootTriggerName = "Shoot"; // trigger no Animator do player (opcional)

    float cdTimer = 0f;
    Camera mainCam;
    Animator playerAnimator;

    void Awake()
    {
        mainCam = Camera.main;
        // tenta pegar animator do player (pai ou avô)
        playerAnimator = GetComponentInParent<Animator>();
    }

    void Update()
    {
        if (cdTimer > 0f) cdTimer -= Time.deltaTime;
    }

    public void Fire()
    {
        Fire(Vector2.zero); // Chama a versão com direção customizada
    }
    
    public void Fire(Vector2 customDirection)
    {
        // Não disparar se o jogo estiver em pausa
        if (PauseManager.IsPaused) return;
        if (cdTimer > 0f) return;

        Vector2 origin = muzzlePoint ? (Vector2)muzzlePoint.position : (Vector2)transform.position;

        Vector2 dir;
        
        // Se tem direção customizada, usa ela (joystick mobile)
        if (customDirection.sqrMagnitude > 0.01f)
        {
            dir = customDirection.normalized;
        }
        else
        {
            // Fallback: calcula direção com o mouse
            Vector2 target = origin;
            if (mainCam != null)
            {
                Vector3 mp = Input.mousePosition;
                Vector3 mw = mainCam.ScreenToWorldPoint(mp);
                target = new Vector2(mw.x, mw.y);
            }
            dir = (target - origin).normalized;
        }

        if (projectilePrefab != null)
        {
            GameObject p = Instantiate(projectilePrefab, origin, Quaternion.identity);
            ProjectileLuka proj = p.GetComponent<ProjectileLuka>();
            // Make projectile pauseable
            if (p.GetComponent<PauseableRigidbody>() == null) p.AddComponent<PauseableRigidbody>();
            if (proj != null) proj.Launch(dir * projectileSpeed);
            else
            {
                Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = dir * projectileSpeed;
            }
        }

        // opcional: dispara trigger de animação do player para recoil/shot flash
        if (playShootAnimationTrigger && playerAnimator != null)
        {
            playerAnimator.SetTrigger(shootTriggerName);
        }

        cdTimer = localCooldown;
        // Sonzinho do tiro: se existe MapAudioManager, ele centraliza os SFX
        if (MapAudioManager.main != null)
        {
            MapAudioManager.main.PlayBow();
        }
        else
        {
            // fallback: se quisermos um som específico por arma, adicione um AudioSource
            // (não alterado por padrão pra evitar conflitos com vocês). Ex:
            // var a = GetComponent<AudioSource>(); if (a != null && shootClip != null) a.PlayOneShot(shootClip);
        }
    }
}
