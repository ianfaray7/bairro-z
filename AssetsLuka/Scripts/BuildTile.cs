using UnityEngine;

public class BuildTile : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] [Range(0f, 1f)] private float sellPricePercentage = 0.5f; // 50% do valor original
    
    [Header("References")]
    [SerializeField] private TowerBuildUI towerBuildUI;
    
    private bool playerNearby = false;
    private bool hasTower = false;
    private SpriteRenderer spriteRenderer;
    private TowerData currentTowerData; // Armazena dados da torre construída
    private bool isUpgraded = false; // Controla se a torre foi upgradada
    
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
        
        // Calcula valor de venda
        int sellPrice = 0;
        if (currentTowerData != null)
        {
            sellPrice = Mathf.RoundToInt(currentTowerData.cost * sellPricePercentage);
        }
        
        Debug.Log($"Torre vendida em {transform.position} por ${sellPrice}");
        hasTower = false;
        currentTowerData = null;
        isUpgraded = false; // Reseta o status de upgrade
        HideSellUI();
        
        // Volta a cor verde
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green;
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
        currentTowerData = towerData; // Salva dados da torre
        HideBuildUI();
        
        // Muda a cor para vermelho para indicar que foi construído
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        
        // Torna o tile sólido (não atravessável)
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }
        
        // FASE 2: Descomente para instanciar torre de verdade
        /*
        if (towerData.towerPrefab != null)
        {
            GameObject towerObj = Instantiate(
                towerData.towerPrefab, 
                transform.position, 
                Quaternion.identity
            );
            
            // Inicializa a torre com os dados
            TowerBehavior tower = towerObj.GetComponent<TowerBehavior>();
            if (tower != null)
            {
                tower.Initialize(towerData);
            }
            
            // Opcional: Define a torre como filho deste tile
            towerObj.transform.SetParent(transform);
        }
        */
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
        
        if (isUpgraded)
        {
            Debug.LogWarning("Torre já foi upgradada!");
            return;
        }
        
        Debug.Log($"Torre em {transform.position} foi upgradada!");
        isUpgraded = true;
        HideSellUI();
        
        // Muda a cor para laranja (upgrade)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.65f, 0f); // Laranja
        }
        
        // FASE 2: Aqui você pode melhorar os stats da torre
        // Por exemplo: aumentar dano, alcance, velocidade, etc.
    }
    
    // Verifica se a torre pode ser upgradada
    public bool CanUpgrade()
    {
        return hasTower && !isUpgraded;
    }
}
