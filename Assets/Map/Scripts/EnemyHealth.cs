using UnityEngine;

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
        Destroy(gameObject, 0.1f);
    }

    public bool IsDead()
    {
        return died;
    }
}
