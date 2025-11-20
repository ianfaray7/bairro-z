using UnityEngine;

public class BuildTile : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] [Range(0f, 1f)] private float sellPricePercentage = 0.5f; // 50% do valor original
    
    [Header("References")]
    [SerializeField] private TowerBuildUI_NOVO towerBuildUI;
    
    private bool playerNearby = false;
    private bool hasTower = false;
    private SpriteRenderer spriteRenderer;
    private TowerData currentTowerData; // Armazena dados da torre construída
    private int currentTowerLevel = 0; // Nível atual da torre (0-4 = níveis 1-5)
    private GameObject currentTowerObject; // Referência ao GameObject da torre
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        // Verifica se o jogador está próximo
        CheckPlayerProximity();
    }
    
    void CheckPlayerProximity()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, playerLayer);
        
        if (hits.Length > 0)
        {
            if (!playerNearby)
            {
                playerNearby = true;
                
                if (hasTower)
                {
                    ShowSellUI();
                }
                else
                {
                    ShowBuildUI();
                }
            }
        }
        else
        {
            if (playerNearby)
            {
                playerNearby = false;
                
                if (hasTower)
                {
                    HideSellUI();
                }
                else
                {
                    HideBuildUI();
                }
            }
        }
    }
    
    void ShowBuildUI()
    {
        if (towerBuildUI != null)
        {
            towerBuildUI.ShowUI(this);
        }
    }
    
    void HideBuildUI()
    {
        if (towerBuildUI != null)
        {
            towerBuildUI.HideUI();
        }
    }
    
    void ShowSellUI()
    {
        if (towerBuildUI != null)
        {
            towerBuildUI.ShowSellUI(this);
        }
    }
    
    void HideSellUI()
    {
        if (towerBuildUI != null)
        {
            towerBuildUI.HideUI();
        }
    }
    
    public void SellTower()
    {
        if (!hasTower)
        {
            Debug.LogWarning("Não há torre para vender!");
            return;
        }
        
        // Calcula valor de venda (custo base + upgrades)
        int totalCost = currentTowerData.baseCost;
        for (int i = 0; i < currentTowerLevel; i++)
        {
            totalCost += currentTowerData.upgradeCosts[i];
        }
        int sellPrice = Mathf.RoundToInt(totalCost * sellPricePercentage);
        
        Debug.Log($"Torre vendida em {transform.position} por ${sellPrice}");
        
        // Toca som de venda
        if (MapAudioManager.main != null)
        {
            MapAudioManager.main.PlayTowerSell();
        }
        
        // Destroi o GameObject da torre
        if (currentTowerObject != null)
        {
            Destroy(currentTowerObject);
        }
        
        hasTower = false;
        currentTowerData = null;
        currentTowerLevel = 0;
        currentTowerObject = null;
        HideSellUI();
        
        // Volta a mostrar o sprite do tile
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            // spriteRenderer.color = Color.green; // REMOVIDO - usa cor original do sprite
        }
        
        // Volta a ser trigger (atravessável)
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Adiciona dinheiro de volta ao jogador
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddMoney(sellPrice);
        }
    }
    
    public void BuildTower(TowerData towerData)
    {
        if (hasTower)
        {
            Debug.LogWarning("Já existe uma torre neste tile!");
            return;
        }
        
        Debug.Log($"Torre {towerData.towerName} construída em {transform.position}");
        hasTower = true;
        currentTowerData = towerData;
        currentTowerLevel = 0; // Começa no nível 1 (índice 0)
        HideBuildUI();
        
        // Esconde o sprite do BuildTile
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Torna o tile sólido (não atravessável)
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }
        
        // Instancia a torre nível 1
        InstantiateTower();
    }
    
    // Visualização no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
    
    // Método público para obter o preço de venda
    public int GetSellPrice()
    {
        if (currentTowerData != null)
        {
            return Mathf.RoundToInt(currentTowerData.cost * sellPricePercentage);
        }
        return 0;
    }
    
    // Método de upgrade da torre
    public void UpgradeTower()
    {
        if (!hasTower)
        {
            Debug.LogWarning("Não há torre para fazer upgrade!");
            return;
        }
        
        if (currentTowerLevel >= 4)
        {
            Debug.LogWarning("Torre já está no nível máximo!");
            return;
        }
        
        // Verifica se tem dinheiro
        int upgradeCost = currentTowerData.upgradeCosts[currentTowerLevel];
        if (ResourceManager.Instance != null)
        {
            if (!ResourceManager.Instance.SpendMoney(upgradeCost))
            {
                Debug.LogWarning("Dinheiro insuficiente para upgrade!");
                return;
            }
        }
        
        currentTowerLevel++;
        Debug.Log($"Torre em {transform.position} upgradada para nível {currentTowerLevel + 1}!");
        
        // Toca som de upgrade
        if (MapAudioManager.main != null)
        {
            MapAudioManager.main.PlayTowerUpgrade();
        }
        
        HideSellUI();
        
        // Atualiza o visual da torre
        InstantiateTower();
    }
    
    // Verifica se a torre pode ser upgradada
    public bool CanUpgrade()
    {
        return hasTower && currentTowerLevel < 4;
    }
    
    // Retorna o custo do próximo upgrade
    public int GetUpgradeCost()
    {
        if (!hasTower || currentTowerLevel >= 4 || currentTowerData == null)
            return 0;
        
        return currentTowerData.upgradeCosts[currentTowerLevel];
    }
    
    // Retorna o nível atual da torre (1-5)
    public int GetTowerLevel()
    {
        return currentTowerLevel + 1;
    }
    
    // Instancia ou atualiza o GameObject da torre
    void InstantiateTower()
    {
        if (currentTowerData == null || currentTowerData.towerLevelPrefabs.Length <= currentTowerLevel)
            return;
        
        // Destroi a torre anterior se existir
        if (currentTowerObject != null)
        {
            Destroy(currentTowerObject);
        }
        
        // Instancia a torre do nível atual
        GameObject prefab = currentTowerData.towerLevelPrefabs[currentTowerLevel];
        if (prefab != null)
        {
            currentTowerObject = Instantiate(prefab, transform.position, Quaternion.identity);
            currentTowerObject.transform.SetParent(transform);
            
            // Inicializa a torre com os dados (se tiver TowerBehavior)
            TowerBehavior tower = currentTowerObject.GetComponent<TowerBehavior>();
            if (tower != null)
            {
                tower.Initialize(currentTowerData);
            }
        }
    }
}
