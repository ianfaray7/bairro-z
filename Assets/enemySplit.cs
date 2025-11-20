using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemySplit : MonoBehaviour, IEnemy
{
    [Header("Path/Movement")]
    public float speed = 2f;
    public float arriveTolerance = 0.1f;

    private Rigidbody2D rb;
    private Animator anim;

    private int index = 0;
    private Transform checkpoint;

    [Header("Animator State Names")]
    public string stateWalkUp = "WalkUp";
    public string stateWalkDown = "WalkDown";
    public string stateWalkLeft = "WalkLeft";
    public string stateWalkRight = "WalkRight";

    // cache pra evitar re-tocar o mesmo estado a cada frame
    private int currentStateHash = 0;

    [Header("Stats")]
    public float maxHealth = 10f;
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
        if (enemySplitManager.main == null || enemySplitManager.main.checkpoints == null || enemySplitManager.main.checkpoints.Length == 0)
        {
            Debug.LogError("enemyManager.main.checkpoints não configurado.", this);
            enabled = false;
            return;
        }

        index = Mathf.Clamp(index, 0, enemySplitManager.main.checkpoints.Length - 1);
        checkpoint = enemySplitManager.main.checkpoints[index];

        if (anim.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator sem Controller. Arraste um Animator Controller para o componente Animator do inimigo.", this);
        }
    }

    void Update()
    {
        // Atualiza referência caso o manager troque os checkpoints em runtime
        if (enemySplitManager.main != null && enemySplitManager.main.checkpoints != null && enemySplitManager.main.checkpoints.Length > 0)
        {
            index = Mathf.Clamp(index, 0, enemyManager.main.checkpoints.Length - 1);
            checkpoint = enemySplitManager.main.checkpoints[index];
        }

        if (checkpoint == null) return;

        // Chegou no checkpoint atual?
        if (Vector2.Distance(transform.position, checkpoint.position) <= arriveTolerance)
        {
            index++;
            if (index >= enemySplitManager.main.checkpoints.Length)
            {
                // Chegou no fim - causa dano à base
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
        // stop movement
        rb.linearVelocity = Vector2.zero;
        // tocar som de death
        if (MapAudioManager.main != null) MapAudioManager.main.PlayZombieDeath();
        // desativa objeto com pequeno delay para animação
        Destroy(gameObject, 0.1f);
    }

    // IEnemy compatibility
    public bool IsDead() => isDead;

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
