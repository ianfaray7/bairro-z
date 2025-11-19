using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gerenciador de recursos do jogador (dinheiro/ouro)
/// Para usar nas próximas fases do projeto
/// </summary>
public class ResourceManager : MonoBehaviour
{
    [Header("Starting Resources")]
    [SerializeField] private int startingMoney = 500;
    
    [Header("UI References (Optional)")]
    [SerializeField] private TextMeshProUGUI moneyText;
    
    private int currentMoney;
    
    // Singleton para fácil acesso
    public static ResourceManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        currentMoney = startingMoney;
        UpdateUI();
    }
    
    /// <summary>
    /// Adiciona dinheiro ao jogador
    /// </summary>
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
        Debug.Log($"Dinheiro adicionado: +{amount}. Total: {currentMoney}");
    }
    
    /// <summary>
    /// Tenta gastar dinheiro. Retorna true se teve sucesso
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            Debug.Log($"Dinheiro gasto: -{amount}. Restante: {currentMoney}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Dinheiro insuficiente! Necessário: {amount}, Atual: {currentMoney}");
            return false;
        }
    }
    
    /// <summary>
    /// Retorna a quantidade atual de dinheiro
    /// </summary>
    public int GetCurrentMoney()
    {
        return currentMoney;
    }
    
    /// <summary>
    /// Verifica se o jogador tem dinheiro suficiente
    /// </summary>
    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }
    
    /// <summary>
    /// Atualiza o texto da UI
    /// </summary>
    void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"${currentMoney}";
        }
    }
}
