using UnityEngine;

[CreateAssetMenu(fileName = "New Tower", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Tower Info")]
    public string towerName;
    public Sprite towerIcon;
    public GameObject towerPrefab;
    
    [Header("Stats")]
    public int cost;
    public float damage;
    public float attackRange;
    public float attackSpeed;
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description;
}
