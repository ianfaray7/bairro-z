using UnityEngine;

/// <summary>
/// Componente para zumbis/inimigos
/// Gerencia vida, morte e efeitos (slow)
/// </summary>
public class EnemyIan : MonoBehaviour, IEnemy
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 15f;
    [SerializeField] private float moveSpeed = 0.2f;
    
    [Header("Movimento de Teste")]
    [SerializeField] private bool enableZigzagMovement = true;
    [SerializeField] private float zigzagSpeed = 1.5f; // Velocidade do zigzag
    [SerializeField] private float zigzagAmount = 0.5f; // Amplitude do zigzag
    
    private float currentHealth;
    private float currentMoveSpeed;
    private bool isDead = false;
    
    // Efeito de slow
    private float slowEffect = 0f; // 0 a 1 (0 = sem slow, 1 = parado)
    private float slowDuration = 0f;
    
    // Efeito visual de dano
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    // Movimento zigzag
    private float zigzagTimer = 0f;
    private Vector3 startPosition;
    
    void Start()
    {
        currentHealth = maxHealth;
        currentMoveSpeed = moveSpeed;
        
        // Pega o SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Guarda posição inicial para zigzag
        startPosition = transform.position;
    }
    
    void Update()
    {
        if (PauseManager.IsPaused) return;
        // Atualiza slow
        if (slowDuration > 0)
        {
            slowDuration -= Time.deltaTime;
            if (slowDuration <= 0)
            {
                slowEffect = 0f;
                currentMoveSpeed = moveSpeed;
            }
        }
        
        // Movimento de zigzag para teste
        if (enableZigzagMovement && !isDead)
        {
            MoveZigzag();
        }
    }
    
    /// <summary>
    /// Movimento de zigzag para testar projéteis em alvos móveis
    /// </summary>
    void MoveZigzag()
    {
        if (PauseManager.IsPaused) return;
        zigzagTimer += Time.deltaTime * zigzagSpeed;
        
        // Movimento para frente (direita)
        float forwardSpeed = currentMoveSpeed;
        transform.position += Vector3.right * forwardSpeed * Time.deltaTime;
        
        // Movimento lateral (zigzag usando seno)
        float lateralOffset = Mathf.Sin(zigzagTimer) * zigzagAmount;
        Vector3 targetPosition = transform.position;
        targetPosition.y = startPosition.y + lateralOffset;
        
        // Suaviza o movimento lateral
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
    }
    
    /// <summary>
    /// Aplica dano ao inimigo
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // Efeito visual de flash vermelho
        if (spriteRenderer != null)
        {
            StopCoroutine("FlashRed");
            StartCoroutine(FlashRed());
        }
        
        Debug.Log($"{gameObject.name} tomou {damage} de dano. Vida: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // som de hit
            if (MapAudioManager.main != null) MapAudioManager.main.PlayZombieHit();
        }
    }
    
    /// <summary>
    /// Efeito de flash vermelho ao receber dano
    /// </summary>
    private System.Collections.IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    /// <summary>
    /// Aplica efeito de slow no inimigo
    /// </summary>
    /// <param name="slowAmount">Quantidade de slow (0 a 1). Ex: 0.5 = 50% mais lento</param>
    /// <param name="duration">Duração do efeito em segundos</param>
    public void ApplySlow(float slowAmount, float duration)
    {
        if (isDead) return;
        
        slowEffect = Mathf.Clamp01(slowAmount);
        slowDuration = duration;
        
        // Reduz velocidade
        currentMoveSpeed = moveSpeed * (1f - slowEffect);
        
        Debug.Log($"{gameObject.name} foi desacelerado em {slowAmount * 100}% por {duration}s");
    }
    
    void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} morreu!");
        
        // 💰 Dar recompensa de moedas ao jogador
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnNormalEnemyKilled();
        }
        
        // toca som de morte
        if (MapAudioManager.main != null) MapAudioManager.main.PlayZombieDeath();
        Destroy(gameObject, 0.1f);
    }
    
    // Getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    public float GetCurrentMoveSpeed() => currentMoveSpeed;
    public float GetSlowEffect() => slowEffect;
    // IEnemy compatibility already provided by IsDead()
}
