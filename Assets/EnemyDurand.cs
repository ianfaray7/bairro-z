using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyDurand : MonoBehaviour, IEnemy
{
    [Header("Path/Movement")]
    public float speed = 2f;
    public float arriveTolerance = 0.1f;

    private Rigidbody2D rb;
    private Animator anim;

    private int index = 0;
    private Transform checkpoint;
    [Header("Path Options")]
    [Tooltip("If true, this enemy will use the split path manager (enemySplitManager) instead of the normal enemyManager. Useful for Map_split.")]
    public bool useSplitPath = false;
    [Tooltip("If true and the current scene is 'Map_split', this enemy will use the split path automatically.")]
    public bool preferSplitOnSplitMap = false;
    
    // Slow effect
    private float originalSpeed;
    private float slowEndTime = 0f;
    private bool isSlowed = false;

    [Header("Animator State Names")]
    public string stateWalkUp = "WalkUp";
    public string stateWalkDown = "WalkDown";
    public string stateWalkLeft = "WalkLeft";
    public string stateWalkRight = "WalkRight";

    // cache pra evitar re-tocar o mesmo estado a cada frame
    private int currentStateHash = 0;

    [Header("Stats")]
    public float maxHealth = 15f;
    private float currentHealth;
    private bool isDead = false;

    // efeito visual
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void Start()
    {
        // auto switch to split path if requested and scene is 'Map_split'
        if (!useSplitPath && preferSplitOnSplitMap)
        {
            string sname = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(sname) && sname.ToLower().Contains("split"))
            {
                useSplitPath = true;
            }
        }

        // Determine which path manager we'll use: enemyManager or enemySplitManager.
        if (useSplitPath)
        {
            if (enemySplitManager.main == null || enemySplitManager.main.checkpoints == null || enemySplitManager.main.checkpoints.Length == 0)
            {
                Debug.LogError("enemySplitManager.main.checkpoints não configurado.", this);
                enabled = false;
                return;
            }

            index = Mathf.Clamp(index, 0, enemySplitManager.main.checkpoints.Length - 1);
            checkpoint = enemySplitManager.main.checkpoints[index];
            Debug.Log($"EnemyDurand: {gameObject.name} will use split path (Map split or wave preference). Manager checkpoints: {enemySplitManager.main.checkpoints.Length}");
        }
        else
        {
            // Try normal manager first
            if (enemyManager.main != null && enemyManager.main.checkpoints != null && enemyManager.main.checkpoints.Length > 0)
            {
                index = Mathf.Clamp(index, 0, enemyManager.main.checkpoints.Length - 1);
                checkpoint = enemyManager.main.checkpoints[index];
            }
            else if (enemySplitManager.main != null && enemySplitManager.main.checkpoints != null && enemySplitManager.main.checkpoints.Length > 0)
            {
                // Fallback to split manager so the enemy won't be disabled if normal manager is missing
                useSplitPath = true;
                index = Mathf.Clamp(index, 0, enemySplitManager.main.checkpoints.Length - 1);
                checkpoint = enemySplitManager.main.checkpoints[index];
                Debug.LogWarning($"EnemyDurand: enemyManager not configured -- falling back to enemySplitManager for {gameObject.name}");
            }
            else
            {
                Debug.LogError("enemyManager.main.checkpoints não configurado.", this);
                enabled = false;
                return;
            }
        }

        if (anim.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator sem Controller. Arraste um Animator Controller para o componente Animator do inimigo.", this);
        }
    }

    void Update()
    {
        // Atualiza referência caso o manager troque os checkpoints em runtime
        if (!useSplitPath)
        {
            if (enemyManager.main != null && enemyManager.main.checkpoints != null && enemyManager.main.checkpoints.Length > 0)
            {
                index = Mathf.Clamp(index, 0, enemyManager.main.checkpoints.Length - 1);
                checkpoint = enemyManager.main.checkpoints[index];
            }
        }
        else
        {
            if (enemySplitManager.main != null && enemySplitManager.main.checkpoints != null && enemySplitManager.main.checkpoints.Length > 0)
            {
                index = Mathf.Clamp(index, 0, enemySplitManager.main.checkpoints.Length - 1);
                checkpoint = enemySplitManager.main.checkpoints[index];
            }
        }
        // no stray extra code

        if (checkpoint == null) return;
        
        // Verifica se o efeito de slow terminou
        if (isSlowed && Time.time >= slowEndTime)
        {
            speed = originalSpeed;
            isSlowed = false;
        }

        // Chegou no checkpoint atual?
        if (Vector2.Distance(transform.position, checkpoint.position) <= arriveTolerance)
        {
            index++;
            if (!useSplitPath)
            {
                if (index >= enemyManager.main.checkpoints.Length)
                {
                    if (BaseHealth.instance != null)
                    {
                        BaseHealth.instance.EnemyReachedBase();
                    }
                    rb.linearVelocity = Vector2.zero;
                    Destroy(gameObject);
                    return;
                }
                checkpoint = enemyManager.main.checkpoints[index];
            }
            else
            {
                if (index >= enemySplitManager.main.checkpoints.Length)
                {
                    if (BaseHealth.instance != null)
                    {
                        BaseHealth.instance.EnemyReachedBase();
                    }
                    rb.linearVelocity = Vector2.zero;
                    Destroy(gameObject);
                    return;
                }
                checkpoint = enemySplitManager.main.checkpoints[index];
            }
            // handled above
        }
    }

    /// <summary>
    /// Aplica dano ao inimigo (pode ser chamado via SendMessage)
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // inicializa vida se necessário
        if (currentHealth <= 0f) currentHealth = maxHealth;
        currentHealth -= damage;

        // efeito visual
        if (spriteRenderer != null)
        {
            StopCoroutine("FlashRed");
            StartCoroutine(FlashRed());
        }

        // toca som de hit
        if (MapAudioManager.main != null) MapAudioManager.main.PlayZombieHit();

        if (currentHealth <= 0f) Die();
    }


    void FixedUpdate()
    {
        if (checkpoint == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toTarget = (Vector2)(checkpoint.position - transform.position);
        Vector2 dir = toTarget.sqrMagnitude > 0.000001f ? toTarget.normalized : Vector2.zero;

        rb.linearVelocity = dir * speed;

        // Atualiza a animação de acordo com a direção dominante
        UpdateWalkAnimation(dir);
    }

    private void UpdateWalkAnimation(Vector2 dir)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return;

        // Se estiver praticamente parado, não troca animação
        if (dir.sqrMagnitude < 0.000001f) return;

        string targetState;

        // Decide pelo eixo dominante (TD geralmente vira em cantos 90°)
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            targetState = dir.x > 0f ? stateWalkRight : stateWalkLeft;
        }
        else
        {
            targetState = dir.y > 0f ? stateWalkUp : stateWalkDown;
        }

        PlayIfDifferent(targetState);

        if (spriteRenderer != null)
        {
            bool isSide = targetState == stateWalkLeft || targetState == stateWalkRight;
            spriteRenderer.flipX = (targetState == stateWalkLeft) && isSide;
        }

    #if UNITY_EDITOR
        Debug.Log($"[EnemyDurand] {gameObject.name} update walk to {targetState} (dir={dir.x:F2},{dir.y:F2}) flipX={spriteRenderer?.flipX}");
    #endif
    }

    private System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        // 💰 Dar recompensa de moedas ao jogador
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnNormalEnemyKilled();
        }
        
        // stop movement
        rb.linearVelocity = Vector2.zero;
        // tocar som de death
        if (MapAudioManager.main != null) MapAudioManager.main.PlayZombieDeath();
        // desativa objeto com pequeno delay para animação
        Destroy(gameObject, 0.1f);
    }

    // IEnemy compatibility
    public bool IsDead() => isDead;
    
    /// <summary>
    /// Aplica efeito de slow (redução de velocidade) ao inimigo
    /// </summary>
    /// <param name="slowAmount">Percentual de redução (0.3 = 30% mais lento)</param>
    /// <param name="duration">Duração do efeito em segundos</param>
    public void ApplySlow(float slowAmount, float duration)
    {
        if (isDead) return;
        
        // Reduz velocidade temporariamente
        StartCoroutine(SlowCoroutine(slowAmount, duration));
    }
    
    private System.Collections.IEnumerator SlowCoroutine(float slowAmount, float duration)
    {
        float originalSpeedValue = speed;
        speed = originalSpeedValue * (1f - slowAmount);
        
        Debug.Log($"⚡ {gameObject.name} sofreu slow de {slowAmount * 100}% por {duration}s");
        
        yield return new WaitForSeconds(duration);
        
        if (!isDead)
        {
            speed = originalSpeedValue;
        }
    }

    private void PlayIfDifferent(string stateName)
    {
        int hash = Animator.StringToHash(stateName);
        if (hash == currentStateHash) return;

        // CrossFade suave; layer 0, sem tempo fixo; transição curta
        anim.CrossFade(hash, 0.05f, 0);
        currentStateHash = hash;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponent<Animator>();
        arriveTolerance = Mathf.Max(0.01f, arriveTolerance);
        speed = Mathf.Max(0f, speed);
        maxHealth = Mathf.Max(1f, maxHealth);
    }
#endif
}
