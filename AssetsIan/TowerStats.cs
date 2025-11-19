using UnityEngine;

/// <summary>
/// Stats de uma torre por nível
/// Usado em TowerData para definir stats de cada nível
/// </summary>
[System.Serializable]
public class TowerStats
{
    [Header("Ataque")]
    public float damage = 10f;
    public float attackSpeed = 1f; // Ataques por segundo
    public float range = 5f;
    
    [Header("Área de Efeito (para Morteiro)")]
    public float aoeRadius = 0f; // 0 = sem área
    
    [Header("Slow (para Torre Elétrica)")]
    public float slowAmount = 0f; // 0 a 1 (0.5 = 50% slow)
    public float slowDuration = 0f; // Duração em segundos
    
    [Header("Projétil")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
}
