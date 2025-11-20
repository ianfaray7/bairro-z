using UnityEngine;
using System;

/// <summary>
/// Simples componente de HP que pode ser adicionado aos prefabs de inimigos do Map
/// - Usa MapAudioManager para tocar sons se presente
/// - Recebe chamadas via SendMessage("TakeDamage", amount)
/// </summary>
public class EnemyHealth : MonoBehaviour, IEnemy
{
    public float maxHealth = 10f;
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
}
