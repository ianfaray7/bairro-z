using UnityEngine;
using System;

public class BaseHealth : MonoBehaviour
{
    [Header("Base Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Damage Settings")]
    public float damagePerEnemy = 10f;
    
    // Singleton para acesso fácil
    public static BaseHealth instance;
    
    // Eventos
    public event Action<float, float> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action OnBaseDestroyed;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Múltiplas instâncias de BaseHealth detectadas!");
            Destroy(this);
        }
    }
    
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"Base tomou {damage} de dano! Vida restante: {currentHealth}/{maxHealth}");
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            BaseDestroyed();
        }
    }
    
    public void EnemyReachedBase()
    {
        TakeDamage(damagePerEnemy);
    }
    
    void BaseDestroyed()
    {
        Debug.Log("Base destruída!");
        OnBaseDestroyed?.Invoke();
    }
    
    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? currentHealth / maxHealth : 0;
    }
    
    public void Heal(float amount)
    {
        if (currentHealth <= 0) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
