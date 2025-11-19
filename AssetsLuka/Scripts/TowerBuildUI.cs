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
            
            // Configura o botão
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                TowerData towerRef = tower; // Captura para o closure
                button.onClick.AddListener(() => OnTowerButtonClicked(towerRef));
            }
            
            // Configura o texto (nome e preço)
            TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
            {
                texts[0].text = $"{tower.towerName}\n${tower.cost}";
            }
            
            // Configura a imagem (se houver)
            Image image = buttonObj.GetComponentInChildren<Image>();
            if (image != null && tower.towerIcon != null)
            {
                image.sprite = tower.towerIcon;
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
        
        // Posiciona a UI próximo ao tile
        Vector3 screenPos = Camera.main.WorldToScreenPoint(buildTile.transform.position);
        uiPanel.transform.position = screenPos + new Vector3(0, 100, 0);
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
        
        // Posiciona a UI próximo ao tile
        Vector3 screenPos = Camera.main.WorldToScreenPoint(buildTile.transform.position);
        uiPanel.transform.position = screenPos + new Vector3(0, 100, 0);
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
        GameObject buttonObj;
        
        // Se tiver prefab específico de venda, usa ele, senão usa o prefab normal
        if (sellButtonPrefab != null)
        {
            buttonObj = Instantiate(sellButtonPrefab, towerButtonsContainer);
        }
        else
        {
            buttonObj = Instantiate(towerButtonPrefab, towerButtonsContainer);
        }
        
        // Configura o botão
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnSellButtonClicked());
        }
        
        // Pega o preço de venda do BuildTile
        int sellPrice = 0;
        if (currentBuildTile != null)
        {
            sellPrice = currentBuildTile.GetSellPrice();
        }
        
        // Configura o texto
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = $"Vender Torre\n ${sellPrice}";
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
        GameObject buttonObj;
        
        // Se tiver prefab específico de upgrade, usa ele, senão usa o prefab normal
        if (upgradeButtonPrefab != null)
        {
            buttonObj = Instantiate(upgradeButtonPrefab, towerButtonsContainer);
        }
        else
        {
            buttonObj = Instantiate(towerButtonPrefab, towerButtonsContainer);
        }
        
        // Configura o botão
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnUpgradeButtonClicked());
        }
        
        // Configura o texto
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = "Upgrade\n";
        }
        
        // Muda a cor do botão para azul/roxo
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
}
