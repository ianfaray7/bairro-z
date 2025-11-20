using UnityEngine;
using System.Collections;

/// <summary>
/// Projétil genérico que segue o alvo e causa dano
/// Suporta movimentação normal e arco (morteiro)
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private bool isArcProjectile = false; // Para morteiro
    [SerializeField] private float arcHeight = 3f; // Altura do arco
    [SerializeField] private float rotationOffset = 0; // Offset de rotação (0 = sprite aponta pra direita, -90 = aponta pra cima)
    
    [Header("Area of Effect (Morteiro)")]
    [SerializeField] private float aoeRadius = 0f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject explosionPrefab; // Animação de explosão
    
    [Header("Slow Effect (Elétrica)")]
    [SerializeField] private float slowAmount = 0f;
    [SerializeField] private float slowDuration = 0f;
    
    private Transform target;
    private Vector3 startPosition;
    private float travelTime = 0f;
    private float journeyLength;
    private bool hasHit = false;
    
    void Start()
    {
        startPosition = transform.position;
        
        if (target != null)
        {
            journeyLength = Vector3.Distance(startPosition, target.position);
        }
    }
    
    void Update()
    {
        if (hasHit) return;
        
        // Se alvo foi destruído
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Movimento
        if (isArcProjectile)
        {
            MoveInArc();
        }
        else
        {
            MoveTowardsTarget();
        }
        
        // Checa colisão
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < 0.2f)
        {
            Hit();
        }
    }
    
    /// <summary>
    /// Movimento normal (direto ao alvo)
    /// </summary>
    void MoveTowardsTarget()
    {
        // Move em direção ao alvo
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Rotaciona para a direção do movimento
        // A ponta da flecha sempre aponta para o alvo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    }
    
    /// <summary>
    /// Movimento em arco (morteiro)
    /// </summary>
    void MoveInArc()
    {
        travelTime += Time.deltaTime * speed;
        
        float fractionOfJourney = travelTime / journeyLength;
        
        if (fractionOfJourney >= 1f)
        {
            Hit();
            return;
        }
        
        // Posição linear
        Vector3 currentPos = Vector3.Lerp(startPosition, target.position, fractionOfJourney);
        
        // Adiciona altura em arco (parábola)
        float arc = arcHeight * Mathf.Sin(fractionOfJourney * Mathf.PI);
        currentPos.y += arc;
        
        transform.position = currentPos;
        
        // Rotaciona baseado na direção do movimento
        if (fractionOfJourney > 0.01f)
        {
            Vector3 direction = (currentPos - startPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90 se sprite aponta para cima
        }
    }
    
    /// <summary>
    /// Acerta o alvo e causa dano
    /// </summary>
    void Hit()
    {
        if (hasHit) return;
        hasHit = true;
        
        // Explosão visual (morteiro)
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1f); // Destroi após 1 segundo
        }
        
        // Dano em área (morteiro)
        if (aoeRadius > 0)
        {
            DamageArea();
        }
        else
        {
            // Dano direto
            DamageTarget(target);
        }
        
        // Destroi projétil
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Causa dano em área (morteiro)
    /// </summary>
    void DamageArea()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, enemyLayer);
        
        foreach (Collider2D hit in hits)
        {
            DamageTarget(hit.transform);
        }
    }
    
    /// <summary>
    /// Causa dano em um alvo específico
    /// </summary>
    void DamageTarget(Transform targetTransform)
    {
        if (targetTransform == null) return;
        
        Enemy enemy = targetTransform.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead())
        {
            // Causa dano (já aciona o flash vermelho automaticamente)
            enemy.TakeDamage(damage);
            
            // Aplica slow (elétrica)
            if (slowAmount > 0 && slowDuration > 0)
            {
                enemy.ApplySlow(slowAmount, slowDuration);
            }
        }
    }
    
    /// <summary>
    /// Configura o projétil (chamado pela torre)
    /// </summary>
    public void Setup(Transform target, float damage, float speed, TowerStats stats)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        
        // Configurações de AoE
        this.aoeRadius = stats.aoeRadius;
        
        // Configurações de slow
        this.slowAmount = stats.slowAmount;
        this.slowDuration = stats.slowDuration;
    }
    
    /// <summary>
    /// Define se é projétil em arco (morteiro)
    /// </summary>
    public void SetArcProjectile(bool isArc, float height = 3f)
    {
        this.isArcProjectile = isArc;
        this.arcHeight = height;
    }
    
    /// <summary>
    /// Define prefab de explosão
    /// </summary>
    public void SetExplosionPrefab(GameObject prefab)
    {
        this.explosionPrefab = prefab;
    }
    
    /// <summary>
    /// Define layer de inimigos
    /// </summary>
    public void SetEnemyLayer(LayerMask layer)
    {
        this.enemyLayer = layer;
    }
}
