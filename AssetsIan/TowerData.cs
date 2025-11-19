using UnityEngine;

[CreateAssetMenu(fileName = "New Tower", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Tower Info")]
    public string towerName;
    
    [Header("Tower Icons")]
    public Sprite towerIconAvailable;    // Ícone quando pode comprar
    public Sprite towerIconUnavailable;  // Ícone quando não pode comprar
    
    [Header("Tower Type")]
    public TowerType towerType;
    
    [Header("Level Prefabs (0-4 = Níveis 1-5)")]
    public GameObject[] towerLevelPrefabs = new GameObject[5];
    
    [Header("Stats por Nível (0-4 = Níveis 1-5)")]
    public TowerStats[] statsPerLevel = new TowerStats[5];
    
    [Header("Costs")]
    public int baseCost;
    public int[] upgradeCosts = new int[4]; // Custo para ir de nível 1->2, 2->3, 3->4, 4->5
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description;
    
    // Métodos para obter stats de um nível específico
    public TowerStats GetStatsForLevel(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, 4); // Nível 1-5 → Index 0-4
        if (statsPerLevel != null && index < statsPerLevel.Length && statsPerLevel[index] != null)
        {
            return statsPerLevel[index];
        }
        
        // Fallback para stats base (compatibilidade)
        var fallbackStats = new TowerStats();
        fallbackStats.damage = baseDamage;
        fallbackStats.range = baseAttackRange;
        fallbackStats.attackSpeed = baseAttackSpeed;
        return fallbackStats;
    }
    
    // Propriedades de compatibilidade
    public int cost => baseCost;
    public GameObject towerPrefab => towerLevelPrefabs.Length > 0 ? towerLevelPrefabs[0] : null;
    public Sprite towerIcon => towerIconAvailable; // Compatibilidade com código antigo
    
    [Header("Stats Base (Nível 1 - DEPRECATED, use statsPerLevel)")]
    public float baseDamage = 10f;
    public float baseAttackRange = 5f;
    public float baseAttackSpeed = 1f;
}

public enum TowerType
{
    Eletrica,
    Morteiro,
    Atirador,
    Arqueira
}
