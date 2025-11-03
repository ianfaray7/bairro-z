using UnityEngine;

/// <summary>
/// Script de exemplo para torres - Para implementar na Fase 2
/// Este é um template básico que pode ser expandido
/// </summary>
public class TowerBehavior : MonoBehaviour
{
    [Header("Tower Data")]
    [SerializeField] private TowerData towerData;
    
    [Header("Attack Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform firePoint;
    
    private float attackCooldown = 0f;
    private Transform currentTarget;
    
    void Start()
    {
        if (towerData != null)
        {
            Debug.Log($"Torre {towerData.towerName} inicializada!");
        }
    }
    
    void Update()
    {
        // Countdown do cooldown de ataque
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
        
        // Procura inimigos no alcance
        FindTarget();
        
        // Ataca se tiver alvo
        if (currentTarget != null && attackCooldown <= 0)
        {
            Attack();
        }
    }
    
    void FindTarget()
    {
        if (towerData == null)
            return;
            
        // Procura todos os inimigos no alcance
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position, 
            towerData.attackRange, 
            enemyLayer
        );
        
        if (enemies.Length > 0)
        {
            // Pega o inimigo mais próximo
            currentTarget = enemies[0].transform;
            
            // Opcional: Rotaciona torre para o alvo
            RotateTowardsTarget();
        }
        else
        {
            currentTarget = null;
        }
    }
    
    void Attack()
    {
        if (towerData == null || currentTarget == null)
            return;
            
        Debug.Log($"Torre {towerData.towerName} atacou {currentTarget.name} causando {towerData.damage} de dano!");
        
        // TODO: Instanciar projétil ou aplicar dano direto
        // Exemplo: Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // Aplica dano (se o inimigo tiver script de vida)
        // var enemy = currentTarget.GetComponent<Enemy>();
        // if (enemy != null)
        //     enemy.TakeDamage(towerData.damage);
        
        // Reseta cooldown
        attackCooldown = towerData.attackSpeed;
    }
    
    void RotateTowardsTarget()
    {
        if (currentTarget == null)
            return;
            
        Vector2 direction = currentTarget.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // Visualização do alcance no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float range = towerData != null ? towerData.attackRange : 5f;
        Gizmos.DrawWireSphere(transform.position, range);
    }
    
    /// <summary>
    /// Inicializa a torre com dados específicos
    /// Chamado pelo BuildTile quando torre é construída
    /// </summary>
    public void Initialize(TowerData data)
    {
        towerData = data;
        Debug.Log($"Torre {towerData.towerName} inicializada com sucesso!");
    }
}
