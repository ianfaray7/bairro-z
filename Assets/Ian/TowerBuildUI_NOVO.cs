using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TowerBuildUI_NOVO : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject uiPanel;
    
    [Header("Botões Fixos de Torres (no Canvas)")]
    [SerializeField] private Button towerButton1; // Torre Elétrica
    [SerializeField] private Button towerButton2; // Torre Morteiro
    [SerializeField] private Button towerButton3; // Torre Atirador
    [SerializeField] private Button towerButton4; // Torre Arqueira
    
    [Header("Textos de Preço dos Botões")]
    [SerializeField] private TextMeshProUGUI priceText1;
    [SerializeField] private TextMeshProUGUI priceText2;
    [SerializeField] private TextMeshProUGUI priceText3;
    [SerializeField] private TextMeshProUGUI priceText4;
    
    [Header("Ícones das Torres")]
    [SerializeField] private Image icon1;
    [SerializeField] private Image icon2;
    [SerializeField] private Image icon3;
    [SerializeField] private Image icon4;
    
    [Header("Torre Data (4 torres fixas - ORDEM: Arqueira, Morteiro, Atirador, Elétrica)")]
    [SerializeField] private TowerData torreArqueira;  // Botão 1 (esquerda)
    [SerializeField] private TowerData torreMorteiro;  // Botão 2
    [SerializeField] private TowerData torreAtirador;  // Botão 3
    [SerializeField] private TowerData torreEletrica;  // Botão 4 (direita)
    
    [Header("Popup de Upgrade/Venda (Container Dinâmico)")]
    [SerializeField] private Transform upgradeButtonsContainer;
    [SerializeField] private GameObject sellButtonPrefab;
    [SerializeField] private GameObject upgradeButtonPrefab;
    
    private BuildTile currentBuildTile;
    private bool isShowingSellUI = false;
    
    void Start()
    {
        // Configura os botões de torre com os listeners
        // ORDEM: Arqueira, Morteiro, Atirador, Elétrica
        if (towerButton1 != null)
            towerButton1.onClick.AddListener(() => OnTowerButtonClicked(torreArqueira));
        
        if (towerButton2 != null)
            towerButton2.onClick.AddListener(() => OnTowerButtonClicked(torreMorteiro));
        
        if (towerButton3 != null)
            towerButton3.onClick.AddListener(() => OnTowerButtonClicked(torreAtirador));
        
        if (towerButton4 != null)
            towerButton4.onClick.AddListener(() => OnTowerButtonClicked(torreEletrica));
        
        // Atualiza os ícones e preços iniciais
        UpdateTowerButtons();
        
        HideUI();
    }
    
    void UpdateTowerButtons()
    {
        // 🎨 SPRITES DISPONÍVEL/INDISPONÍVEL:
        // Cada TowerData tem 2 sprites: towerIconAvailable (aceso) e towerIconUnavailable (apagado)
        // Verifica o dinheiro do jogador para decidir qual ícone mostrar
        
        int currentMoney = ResourceManager.Instance != null ? ResourceManager.Instance.GetCurrentMoney() : 0;
        
        // Atualiza Torre 1 (Arqueira)
        if (torreArqueira != null)
        {
            bool canAfford = currentMoney >= torreArqueira.baseCost;
            if (icon1 != null)
            {
                icon1.sprite = canAfford ? torreArqueira.towerIconAvailable : torreArqueira.towerIconUnavailable;
                icon1.color = Color.white;
            }
            if (priceText1 != null)
            {
                priceText1.text = $"${torreArqueira.baseCost}";
                priceText1.color = canAfford ? Color.white : new Color(1f, 0.5f, 0.5f); // Vermelho claro se não pode comprar
            }
        }
        
        // Atualiza Torre 2 (Morteiro)
        if (torreMorteiro != null)
        {
            bool canAfford = currentMoney >= torreMorteiro.baseCost;
            if (icon2 != null)
            {
                icon2.sprite = canAfford ? torreMorteiro.towerIconAvailable : torreMorteiro.towerIconUnavailable;
                icon2.color = Color.white;
            }
            if (priceText2 != null)
            {
                priceText2.text = $"${torreMorteiro.baseCost}";
                priceText2.color = canAfford ? Color.white : new Color(1f, 0.5f, 0.5f);
            }
        }
        
        // Atualiza Torre 3 (Atirador)
        if (torreAtirador != null)
        {
            bool canAfford = currentMoney >= torreAtirador.baseCost;
            if (icon3 != null)
            {
                icon3.sprite = canAfford ? torreAtirador.towerIconAvailable : torreAtirador.towerIconUnavailable;
                icon3.color = Color.white;
            }
            if (priceText3 != null)
            {
                priceText3.text = $"${torreAtirador.baseCost}";
                priceText3.color = canAfford ? Color.white : new Color(1f, 0.5f, 0.5f);
            }
        }
        
        // Atualiza Torre 4 (Elétrica)
        if (torreEletrica != null)
        {
            bool canAfford = currentMoney >= torreEletrica.baseCost;
            if (icon4 != null)
            {
                icon4.sprite = canAfford ? torreEletrica.towerIconAvailable : torreEletrica.towerIconUnavailable;
                icon4.color = Color.white;
            }
            if (priceText4 != null)
            {
                priceText4.text = $"${torreEletrica.baseCost}";
                priceText4.color = canAfford ? Color.white : new Color(1f, 0.5f, 0.5f);
            }
        }
    }
    
    public void ShowUI(BuildTile buildTile)
    {
        currentBuildTile = buildTile;
        isShowingSellUI = false;
        
        // Limpa botões de upgrade/venda se existirem
        ClearUpgradeButtons();
        
        // Mostra os 4 botões de torre
        ShowTowerButtons(true);
        
        // Atualiza os ícones baseado no dinheiro atual
        UpdateTowerButtons();
        
        // Ajusta tamanho do painel para 4 torres (largo)
        ResizeBuildPanel(520, 140);
        
        uiPanel.SetActive(true);
        
        // Posiciona a UI acima do tile
        PositionUIAboveTile(buildTile);
    }
    
    public void ShowSellUI(BuildTile buildTile)
    {
        currentBuildTile = buildTile;
        isShowingSellUI = true;
        
        // Esconde os botões de torre
        ShowTowerButtons(false);
        
        // Limpa botões anteriores
        ClearUpgradeButtons();
        
        // Ajusta tamanho do painel para 2 botões (menor)
        ResizeBuildPanel(300, 140);
        
        // ORDEM: Upgrade primeiro (esquerda), Venda depois (direita)
        if (buildTile.CanUpgrade())
        {
            CreateUpgradeButton();
        }
        
        CreateSellButton();
        
        uiPanel.SetActive(true);
        
        // Posiciona a UI acima do tile
        PositionUIAboveTile(buildTile);
    }
    
    void ShowTowerButtons(bool show)
    {
        if (towerButton1 != null) towerButton1.gameObject.SetActive(show);
        if (towerButton2 != null) towerButton2.gameObject.SetActive(show);
        if (towerButton3 != null) towerButton3.gameObject.SetActive(show);
        if (towerButton4 != null) towerButton4.gameObject.SetActive(show);
    }
    
    void ClearUpgradeButtons()
    {
        if (upgradeButtonsContainer == null) return;
        
        foreach (Transform child in upgradeButtonsContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    void CreateSellButton()
    {
        if (upgradeButtonsContainer == null || sellButtonPrefab == null) return;
        
        GameObject buttonObj = Instantiate(sellButtonPrefab, upgradeButtonsContainer);
        
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnSellButtonClicked());
        }
        
        int sellPrice = currentBuildTile != null ? currentBuildTile.GetSellPrice() : 0;
        
        Transform priceTextTransform = buttonObj.transform.Find("PriceText");
        if (priceTextTransform != null)
        {
            TextMeshProUGUI priceText = priceTextTransform.GetComponent<TextMeshProUGUI>();
            if (priceText != null)
            {
                priceText.text = $"${sellPrice}";
            }
        }
    }
    
    void CreateUpgradeButton()
    {
        if (upgradeButtonsContainer == null || upgradeButtonPrefab == null) return;
        
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonsContainer);
        
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnUpgradeButtonClicked());
        }
        
        int currentLevel = currentBuildTile != null ? currentBuildTile.GetTowerLevel() : 1;
        int upgradeCost = currentBuildTile != null ? currentBuildTile.GetUpgradeCost() : 0;
        
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
    }
    
    void OnSellButtonClicked()
    {
        if (currentBuildTile != null)
        {
            currentBuildTile.SellTower();
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
        if (currentBuildTile == null || towerData == null) return;
        
        // Verificação de dinheiro
        if (ResourceManager.Instance != null)
        {
            if (ResourceManager.Instance.SpendMoney(towerData.baseCost))
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
    
    void PositionUIAboveTile(BuildTile buildTile)
    {
        if (buildTile == null || uiPanel == null)
            return;
        
        // Canvas é World Space, então posiciona diretamente no mundo
        Vector3 tileWorldPos = buildTile.transform.position;
        
        // Offset vertical - ajustado para Canvas Scale 0.1
        // (Se Canvas Scale = 0.01, use 2f. Se Scale = 0.1, use 1.5f)
        Vector3 popupPos = tileWorldPos + new Vector3(0, 1.5f, 0);
        
        // Posiciona o painel no mundo
        uiPanel.transform.position = popupPos;
        
        // Faz o popup sempre olhar para a câmera (billboard)
        if (Camera.main != null)
        {
            uiPanel.transform.LookAt(Camera.main.transform);
            uiPanel.transform.Rotate(0, 180, 0); // Vira para frente
        }
    }
    
    void ResizeBuildPanel(float width, float height)
    {
        if (uiPanel == null) return;
        
        RectTransform rectTransform = uiPanel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}
