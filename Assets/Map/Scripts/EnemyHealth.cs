using UnityEngine;
using System;

/// <summary>
/// Simples componente de HP que pode ser adicionado aos prefabs de inimigos do Map
/// - Usa MapAudioManager para tocar sons se presente
/// - Recebe chamadas via SendMessage("TakeDamage", amount)
/// </summary>
public class EnemyHealth : MonoBehaviour, IEnemy
{
    public float maxHealth = 15f;
    public float currentHealth;
    public bool died = false;

    // Evento para notificar quando o inimigo morrer
    public event Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // público para ser chamado por SendMessage
    public void TakeDamage(float damage)
    {
        if (died) return;
        currentHealth -= damage;
        if (MapAudioManager.main != null) MapAudioManager.main.PlayZombieHit();
        if (currentHealth <= 0f) Die();
    }
    // IEnemy compatibility — method above already implements it

    void Die()
    {
        died = true;
        if (MapAudioManager.main != null) MapAudioManager.main.PlayZombieDeath();
        
        // Dispara o evento antes de destruir
        OnDeath?.Invoke();
        
        Destroy(gameObject, 0.1f);
    }

    public bool IsDead()
    {
        return died;
    }
    
    /// <summary>
    /// Aplica efeito de slow (redução de velocidade) ao inimigo
    /// Requer que o inimigo tenha campo 'speed' público para funcionar
    /// </summary>
    public void ApplySlow(float slowAmount, float duration)
    {
        if (died) return;
        
        // Tenta encontrar campo speed em scripts de movimento comuns
        var durand = GetComponent<EnemyDurand>();
        if (durand != null)
        {
            durand.ApplySlow(slowAmount, duration);
            return;
        }
        
        var voador = GetComponent<enemyVoador>();
        if (voador != null)
        {
            voador.ApplySlow(slowAmount, duration);
            return;
        }
        
        var split = GetComponent<EnemySplit>();
        if (split != null)
        {
            split.ApplySlow(slowAmount, duration);
            return;
        }
        
        var voadorSplit = GetComponent<enemyVoadorSplit>();
        if (voadorSplit != null)
        {
            voadorSplit.ApplySlow(slowAmount, duration);
            return;
        }
        
        Debug.LogWarning($"ApplySlow chamado em {gameObject.name} mas nenhum script de movimento compatível encontrado.");
    }
}
