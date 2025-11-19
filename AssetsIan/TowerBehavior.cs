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
    
    [Header("Animation (Optional)")]
    [SerializeField] private Animator animator;
    
    private float attackCooldown = 0f;
    private Transform currentTarget;
    private bool hasAnimator = false;
    
    void Start()
    {
        // Tenta pegar o Animator se não foi atribuído
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        hasAnimator = animator != null;
        
        if (towerData != null)
        {
            Debug.Log($"Torre {towerData.towerName} inicializada!");
        }
        
        // Inicia na animação Idle se tiver animator
        if (hasAnimator)
        {
            animator.SetBool("IsIdle", true);
            animator.SetBool("IsShooting", false);
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
        
        // Atualiza animação baseado no estado
        UpdateAnimation();
    }
    
    void UpdateAnimation()
    {
        if (!hasAnimator)
            return;
        
        // Se tem alvo, fica atirando, senão fica Idle
        bool isShooting = currentTarget != null;
        animator.SetBool("IsShooting", isShooting);
        animator.SetBool("IsIdle", !isShooting);
    }
    
    void FindTarget()
    {
        if (towerData == null)
            return;
            
        // Procura todos os inimigos no alcance
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position, 
            towerData.baseAttackRange, 
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
        
        // Trigger da animação de ataque (se tiver)
        if (hasAnimator)
        {
            animator.SetTrigger("Attack");
        }
            
        Debug.Log($"Torre {towerData.towerName} atacou {currentTarget.name} causando {towerData.baseDamage} de dano!");
        
        // TODO: Instanciar projétil ou aplicar dano direto
        // Exemplo: Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // Aplica dano (se o inimigo tiver script de vida)
        // var enemy = currentTarget.GetComponent<Enemy>();
        // if (enemy != null)
        //     enemy.TakeDamage(towerData.baseDamage);
        
        // Reseta cooldown
        attackCooldown = towerData.baseAttackSpeed;
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
        float range = towerData != null ? towerData.baseAttackRange : 5f;
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
