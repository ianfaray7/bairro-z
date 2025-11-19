using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerBuildUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Transform towerButtonsContainer;
    [SerializeField] private GameObject towerButtonPrefab;
    [SerializeField] private GameObject sellButtonPrefab; // Prefab do botão de venda
    [SerializeField] private GameObject upgradeButtonPrefab; // Prefab do botão de upgrade
    
    [Header("Tower Data")]
    [SerializeField] private TowerData[] availableTowers;
    
    private BuildTile currentBuildTile;
    private bool isShowingSellUI = false;
    
    void Start()
    {
        HideUI();
    }
    
    void CreateTowerButtons()
    {
        // Limpa botões existentes
        foreach (Transform child in towerButtonsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Cria um botão para cada torre disponível
        foreach (TowerData tower in availableTowers)
        {
            GameObject buttonObj = Instantiate(towerButtonPrefab, towerButtonsContainer);
            
            // Verifica se jogador tem dinheiro suficiente
            bool canAfford = true; // Por enquanto sempre true (sem sistema de moedas)
            
            // TODO: Quando tiver ResourceManager ativo, descomentar:
            // if (ResourceManager.Instance != null)
            // {
            //     canAfford = ResourceManager.Instance.GetMoney() >= tower.baseCost;
            // }
            
            // Configura o botão
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                TowerData towerRef = tower; // Captura para o closure
                button.onClick.AddListener(() => OnTowerButtonClicked(towerRef));
                
                // Desabilita botão se não pode comprar
                button.interactable = canAfford;
            }
            
            // Configura a imagem do ícone (procura especificamente "Icon")
            Transform iconTransform = buttonObj.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    if (canAfford && tower.towerIconAvailable != null)
                    {
                        iconImage.sprite = tower.towerIconAvailable;
                        iconImage.color = Color.white;
                    }
                    else if (!canAfford && tower.towerIconUnavailable != null)
                    {
                        iconImage.sprite = tower.towerIconUnavailable;
                        iconImage.color = Color.white;
                    }
                    else if (tower.towerIconAvailable != null)
                    {
                        iconImage.sprite = tower.towerIconAvailable;
                        iconImage.color = new Color(0.5f, 0.5f, 0.5f);
                    }
                }
            }
            
            // Configura o texto de preço (procura especificamente "PriceText")
            Transform priceTextTransform = buttonObj.transform.Find("PriceText");
            if (priceTextTransform != null)
            {
                TextMeshProUGUI priceText = priceTextTransform.GetComponent<TextMeshProUGUI>();
                if (priceText != null)
                {
                    priceText.text = $"${tower.baseCost}";
                    
                    if (!canAfford)
                    {
                        priceText.color = new Color(0.5f, 0.5f, 0.5f);
                    }
                }
            }
        }
    }
    
    public void ShowUI(BuildTile buildTile)
    {
        currentBuildTile = buildTile;
        isShowingSellUI = false;
        
        // Limpa botões anteriores
        ClearButtons();
        
        // Cria botões de torre
        CreateTowerButtons();
        
        uiPanel.SetActive(true);
        
        // Posiciona a UI FIXA acima do tile (mundo, não tela)
        PositionUIAboveTile(buildTile);
    }
    
    public void ShowSellUI(BuildTile buildTile)
    {
        currentBuildTile = buildTile;
        isShowingSellUI = true;
        
        // Limpa botões anteriores
        ClearButtons();
        
        // Cria botão de venda
        CreateSellButton();
        
        // Cria botão de upgrade apenas se a torre ainda não foi upgradada
        if (buildTile.CanUpgrade())
        {
            CreateUpgradeButton();
        }
        
        uiPanel.SetActive(true);
        
        // Posiciona a UI FIXA acima do tile (mundo, não tela)
        PositionUIAboveTile(buildTile);
    }
    
    void ClearButtons()
    {
        foreach (Transform child in towerButtonsContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    void CreateSellButton()
    {
        GameObject buttonObj = Instantiate(
            sellButtonPrefab != null ? sellButtonPrefab : towerButtonPrefab,
            towerButtonsContainer
        );
        
        // Configura o botão
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnSellButtonClicked());
        }
        
        // Pega o preço de venda do BuildTile
        int sellPrice = currentBuildTile != null ? currentBuildTile.GetSellPrice() : 0;
        
        // Configura o texto de preço (procura "PriceText")
        Transform priceTextTransform = buttonObj.transform.Find("PriceText");
        if (priceTextTransform != null)
        {
            TextMeshProUGUI priceText = priceTextTransform.GetComponent<TextMeshProUGUI>();
            if (priceText != null)
            {
                priceText.text = $"${sellPrice}";
            }
        }
        
        // Opcional: Muda a cor do botão para indicar venda
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = new Color(1f, 0.5f, 0f); // Laranja
        }
    }
    
    void OnSellButtonClicked()
    {
        if (currentBuildTile != null)
        {
            currentBuildTile.SellTower();
        }
    }
    
    void CreateUpgradeButton()
    {
        GameObject buttonObj = Instantiate(
            upgradeButtonPrefab != null ? upgradeButtonPrefab : towerButtonPrefab,
            towerButtonsContainer
        );
        
        // Configura o botão
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnUpgradeButtonClicked());
        }
        
        // Pega informações do upgrade
        int currentLevel = currentBuildTile != null ? currentBuildTile.GetTowerLevel() : 1;
        int upgradeCost = currentBuildTile != null ? currentBuildTile.GetUpgradeCost() : 0;
        
        // Procura "LevelText" primeiro, senão usa "PriceText"
        Transform textTransform = buttonObj.transform.Find("LevelText");
        if (textTransform == null)
            textTransform = buttonObj.transform.Find("PriceText");
        
        if (textTransform != null)
        {
            TextMeshProUGUI levelText = textTransform.GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                levelText.text = $"Lv.{currentLevel}→{currentLevel + 1}\n${upgradeCost}";
            }
        }
        
        // Muda a cor do botão para azul
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = new Color(0.5f, 0.5f, 1f); // Azul claro
        }
    }
    
    void OnUpgradeButtonClicked()
    {
        if (currentBuildTile != null)
        {
            currentBuildTile.UpgradeTower();
        }
    }
    
    public void HideUI()
    {
        uiPanel.SetActive(false);
        currentBuildTile = null;
        isShowingSellUI = false;
    }
    
    void OnTowerButtonClicked(TowerData towerData)
    {
        if (currentBuildTile != null)
        {
            // Verificação de dinheiro (se ResourceManager existir)
            if (ResourceManager.Instance != null)
            {
                if (ResourceManager.Instance.SpendMoney(towerData.cost))
                {
                    currentBuildTile.BuildTower(towerData);
                }
                else
                {
                    Debug.LogWarning("Dinheiro insuficiente para construir esta torre!");
                }
            }
            else
            {
                // Se não houver ResourceManager, constrói direto
                currentBuildTile.BuildTower(towerData);
            }
        }
    }
    
    // Posiciona a UI fixa acima do tile
    void PositionUIAboveTile(BuildTile buildTile)
    {
        if (buildTile == null || Camera.main == null)
            return;
        
        // Converte posição do tile para posição de tela
        Vector3 tileWorldPos = buildTile.transform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(tileWorldPos);
        
        // Adiciona offset vertical fixo (100 pixels acima do tile)
        screenPos.y += 100f;
        
        // Aplica a posição ao painel
        uiPanel.transform.position = screenPos;
    }
}
