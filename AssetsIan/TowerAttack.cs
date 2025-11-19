using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Comportamento de ataque das torres
/// Suporta: ataque direto, área (Morteiro), slow (Elétrica)
/// Stats escaláveis por nível
/// </summary>
public class TowerAttack : MonoBehaviour
{
    [Header("Torre Configuration")]
    [SerializeField] private TowerData towerData;
    [SerializeField] private int currentLevel = 1; // 1 a 5
    
    [Header("Detection")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform firePoint; // Ponto de onde sai o projétil
    
    [Header("Animation")]
    [SerializeField] private Animator shooterAnimator; // Animator do personagem (Atirador/Arqueira)
    [SerializeField] private SpriteRenderer shooterSpriteRenderer; // SpriteRenderer do personagem para espelhamento
    [SerializeField] private float baseAnimationDuration = 0.5f; // Duração base da animação de tiro em segundos
    
    [Header("Visual Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    // Estado
    private TowerStats currentStats;
    private float attackCooldown = 0f;
    private Enemy currentTarget;
    private List<Enemy> enemiesInRange = new List<Enemy>();
    
    void Start()
    {
        // Carrega stats do nível atual
        LoadStatsForCurrentLevel();
        
        // Se não tem firePoint, usa a posição da torre
        if (firePoint == null)
            firePoint = transform;
    }
    
    void Update()
    {
        // Remove inimigos mortos da lista
        enemiesInRange.RemoveAll(e => e == null || e.IsDead());
        
        // Cooldown de ataque
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            return;
        }
        
        // Busca alvo
        FindTarget();
        
        // Ataca se tiver alvo
        if (currentTarget != null && !currentTarget.IsDead())
        {
            Attack();
        }
    }
    
    void LoadStatsForCurrentLevel()
    {
        if (towerData != null)
        {
            currentStats = towerData.GetStatsForLevel(currentLevel);
            Debug.Log($"{towerData.towerName} Level {currentLevel} carregado. Dano: {currentStats.damage}, Range: {currentStats.range}");
            
            // Atualiza velocidade da animação quando stats mudam
            UpdateAnimationSpeed();
        }
        else
        {
            Debug.LogError("TowerData não atribuído!");
        }
    }
    
    void FindTarget()
    {
        // Busca inimigos no raio
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentStats.range, enemyLayer);
        
        // Limpa lista e adiciona novos inimigos
        enemiesInRange.Clear();
        Enemy closestEnemy = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead())
            {
                enemiesInRange.Add(enemy);
                
                // Encontra o mais próximo
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }
        
        currentTarget = closestEnemy;
    }
    
    void Attack()
    {
        // Define cooldown
        attackCooldown = 1f / currentStats.attackSpeed;
        
        // Atualiza direção do personagem (Atirador/Arqueira)
        if (towerData.towerType == TowerType.Atirador || towerData.towerType == TowerType.Arqueira)
        {
            UpdateShooterDirection();
            
            // Ativa animação de tiro
            if (shooterAnimator != null)
            {
                // Usa Trigger ao invés de Bool para garantir que a animação roda sempre
                shooterAnimator.SetTrigger("Shoot");
                
                // Spawna projétil no momento correto da animação (50% da animação)
                StartCoroutine(SpawnProjectileAtAnimationTime(baseAnimationDuration * 0.5f));
            }
            else
            {
                // Fallback se não tiver animator
                SpawnProjectile();
            }
        }
        else
        {
            // Outras torres (Morteiro, Elétrica) - ataque direto
            if (shooterAnimator != null)
            {
                shooterAnimator.SetTrigger("Shoot");
            }
            
            SpawnProjectile();
        }
    }
    
    /// <summary>
    /// Atualiza a velocidade da animação baseado no attackSpeed
    /// Garante que a animação termine antes do próximo ataque
    /// </summary>
    void UpdateAnimationSpeed()
    {
        if (shooterAnimator == null || currentStats == null)
        {
            return;
        }
        
        // Calcula velocidade da animação
        // attackSpeed é ataques por segundo
        // Tempo entre ataques = 1 / attackSpeed
        // A animação precisa terminar ANTES do próximo ataque
        
        float timeBetweenAttacks = 1f / currentStats.attackSpeed;
        
        // A animação deve ocupar no máximo 70% do tempo entre ataques
        // Isso deixa um buffer para garantir que não há sobreposição
        float maxAnimationTime = timeBetweenAttacks * 0.5f;
        
        // Se a animação base demora mais que o tempo disponível, acelera
        // Se demora menos, desacelera (mas não muito)
        // Velocidade = quanto a animação base precisa ser acelerada
        float animationSpeed = baseAnimationDuration / maxAnimationTime;
        
        // Clamp para evitar animações muito rápidas ou muito lentas
        // Mínimo 0.8x (um pouco mais lento que normal)
        // Máximo 5x (bem rápido para torres de alta velocidade)
        animationSpeed = Mathf.Clamp(animationSpeed, 0.8f, 5f);
        
        shooterAnimator.speed = animationSpeed;
        
        Debug.Log($"Velocidade da animação: {animationSpeed:F2}x | AttackSpeed: {currentStats.attackSpeed:F2}/s | Tempo entre ataques: {timeBetweenAttacks:F2}s | Max anim: {maxAnimationTime:F2}s");
    }
    
    /// <summary>
    /// Spawna projétil em um momento específico da animação
    /// </summary>
    IEnumerator SpawnProjectileAtAnimationTime(float delay)
    {
        if (shooterAnimator != null && shooterAnimator.speed > 0)
        {
            // Aguarda o momento correto da animação (ajustado pela velocidade do animator)
            float adjustedDelay = delay / shooterAnimator.speed;
            yield return new WaitForSeconds(adjustedDelay);
        }
        else
        {
            // Fallback - aguarda metade do delay base
            yield return new WaitForSeconds(delay);
        }
        
        SpawnProjectile();
    }
    
    /// <summary>
    /// Atualiza a direção e animação do personagem atirador baseado na posição do inimigo
    /// </summary>
    void UpdateShooterDirection()
    {
        if (currentTarget == null || shooterAnimator == null) return;
        
        // Calcula direção do inimigo em relação à torre
        Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
        
        // Calcula ângulo em graus
        // Atan2 retorna: 0°=Direita, 90°=Cima, -90°=Baixo, 180°=Esquerda
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Determina a direção e se precisa espelhar
        int directionIndex = 0; // 0=Sul, 1=Sudoeste, 2=Oeste, 3=Noroeste/Nordeste, 4=Norte
        bool flipX = false;
        
        // Verifica se é Atirador (sprites invertidos em relação à Arqueira)
        bool isAtirador = (towerData.towerType == TowerType.Atirador);
        
        // Divide em 8 setores de 45 graus cada
        if (angle >= 67.5f && angle < 112.5f)
        {
            // Norte (cima) - 90°
            directionIndex = 4;
            flipX = false;
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            // Nordeste (cima-direita) - 45°
            directionIndex = 3;
            // Atirador: sprite é Nordeste (não espelha)
            // Arqueira: espelha Noroeste
            flipX = !isAtirador;
        }
        else if (angle >= -22.5f && angle < 22.5f)
        {
            // Leste (direita) - 0° - espelha Oeste
            directionIndex = 2;
            flipX = true;
        }
        else if (angle >= -67.5f && angle < -22.5f)
        {
            // Sudeste (baixo-direita) - -45° - espelha Sudoeste
            directionIndex = 1;
            flipX = true;
        }
        else if (angle >= -112.5f && angle < -67.5f)
        {
            // Sul (baixo) - -90°
            directionIndex = 0;
            flipX = false;
        }
        else if (angle >= -157.5f && angle < -112.5f)
        {
            // Sudoeste (baixo-esquerda) - -135°
            directionIndex = 1;
            flipX = false;
        }
        else if (angle >= 157.5f || angle < -157.5f)
        {
            // Oeste (esquerda) - 180°/-180°
            directionIndex = 2;
            flipX = false;
        }
        else // angle >= 112.5f && angle < 157.5f
        {
            // Noroeste (cima-esquerda) - 135°
            directionIndex = 3;
            // Atirador: espelha Nordeste
            // Arqueira: sprite é Noroeste (não espelha)
            flipX = isAtirador;
        }
        
        // Atualiza parâmetro de direção no Animator
        shooterAnimator.SetInteger("Direction", directionIndex);
        
        // Aplica espelhamento no SpriteRenderer
        if (shooterSpriteRenderer != null)
        {
            shooterSpriteRenderer.flipX = flipX;
        }
        
        Debug.Log($"{towerData.towerName} apontando: direção={directionIndex}, espelhado={flipX}, ângulo={angle:F1}°");
    }
    
    /// <summary>
    /// Spawna projétil em direção ao alvo
    /// </summary>
    void SpawnProjectile()
    {
        if (currentTarget == null || currentStats.projectilePrefab == null) return;
        
        // Instancia projétil na posição do firePoint
        GameObject projectileObj = Instantiate(currentStats.projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            // Configura o projétil
            projectile.Setup(currentTarget.transform, currentStats.damage, currentStats.projectileSpeed, currentStats);
            projectile.SetEnemyLayer(enemyLayer);
            
            // Configurações especiais por tipo de torre
            switch (towerData.towerType)
            {
                case TowerType.Morteiro:
                    // Morteiro usa arco e tem explosão
                    projectile.SetArcProjectile(true, 3f);
                    // TODO: Configurar explosionPrefab se existir
                    break;
                    
                case TowerType.Eletrica:
                case TowerType.Atirador:
                case TowerType.Arqueira:
                    // Projéteis diretos
                    projectile.SetArcProjectile(false);
                    break;
            }
            
            Debug.Log($"{towerData.towerName} disparou projétil contra {currentTarget.name}");
        }
        else
        {
            Debug.LogError($"Prefab de projétil não tem componente Projectile!");
        }
    }
    
    /// <summary>
    /// Upgrade da torre para próximo nível
    /// </summary>
    public void UpgradeLevel()
    {
        if (currentLevel < 5)
        {
            currentLevel++;
            LoadStatsForCurrentLevel();
            Debug.Log($"{towerData.towerName} upgradado para Level {currentLevel}");
        }
    }
    
    /// <summary>
    /// Define nível manualmente
    /// </summary>
    public void SetLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 5);
        LoadStatsForCurrentLevel();
    }
    
    // Getters
    public int GetCurrentLevel() => currentLevel;
    public TowerStats GetCurrentStats() => currentStats;
    public int GetEnemiesInRangeCount() => enemiesInRange.Count;
    
    // Debug Visual
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || currentStats == null) return;
        
        // Raio de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentStats.range);
        
        // Área de efeito (Morteiro)
        if (currentStats.aoeRadius > 0 && currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentTarget.transform.position, currentStats.aoeRadius);
        }
        
        // Linha para o alvo
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}
