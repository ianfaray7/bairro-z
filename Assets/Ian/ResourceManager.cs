using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Gerenciador de recursos do jogador (dinheiro, vidas, waves)
/// Sistema completo de monetização e game state
/// </summary>
public class ResourceManager : MonoBehaviour
{
    [Header("Starting Resources")]
    [SerializeField] private int startingMoney = 500;
    [SerializeField] private int startingLives = 15;
    
    [Header("Wave Rewards")]
    [SerializeField] private int moneyPerWave = 150;
    
    [Header("Enemy Rewards - FÁCIL DE AJUSTAR PARA BALANCEAMENTO")]
    [Tooltip("Moedas ganhas ao matar um inimigo normal")]
    [SerializeField] private int normalEnemyReward = 10;
    [Tooltip("Moedas ganhas ao matar um inimigo voador")]
    [SerializeField] private int flyingEnemyReward = 15;
    [Tooltip("Moedas ganhas ao matar um inimigo que se divide")]
    [SerializeField] private int splitEnemyReward = 20;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;
    
    [Header("UI Icons (Optional)")]
    [SerializeField] private UnityEngine.UI.Image moneyIcon;
    [SerializeField] private UnityEngine.UI.Image livesIcon;
    [SerializeField] private UnityEngine.UI.Image waveIcon;
    
    // Estado do jogo
    private int currentMoney;
    private int currentLives;
    private int currentWave = 0;
    
    // Eventos para notificar outras partes do jogo
    public UnityEvent OnLivesChanged;
    public UnityEvent OnGameOver;
    public UnityEvent OnMoneyChanged;
    
    // Singleton para fácil acesso
    public static ResourceManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Não destruir ao trocar de cena
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        currentMoney = startingMoney;
        currentLives = startingLives;
        currentWave = 0;
        UpdateAllUI();
    }
    
    #region Money Methods
    
    /// <summary>
    /// Adiciona dinheiro ao jogador
    /// </summary>
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
        OnMoneyChanged?.Invoke();
        Debug.Log($"💰 Dinheiro adicionado: +{amount}. Total: {currentMoney}");
    }
    
    /// <summary>
    /// Tenta gastar dinheiro. Retorna true se teve sucesso
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            OnMoneyChanged?.Invoke();
            Debug.Log($"💸 Dinheiro gasto: -{amount}. Restante: {currentMoney}");
            return true;
        }
        else
        {
            Debug.LogWarning($"❌ Dinheiro insuficiente! Necessário: {amount}, Atual: {currentMoney}");
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
    
    #endregion
    
    #region Lives Methods
    
    /// <summary>
    /// Remove uma vida do jogador (chamado quando inimigo chega ao fim)
    /// </summary>
    public void LoseLife(int amount = 1)
    {
        currentLives -= amount;
        if (currentLives < 0) currentLives = 0;
        
        UpdateLivesUI();
        OnLivesChanged?.Invoke();
        
        Debug.Log($"❤️ Vida perdida! Vidas restantes: {currentLives}");
        
        if (currentLives <= 0)
        {
            GameOver();
        }
    }
    
    /// <summary>
    /// Adiciona vidas ao jogador
    /// </summary>
    public void AddLives(int amount)
    {
        currentLives += amount;
        UpdateLivesUI();
        OnLivesChanged?.Invoke();
        Debug.Log($"❤️ Vidas adicionadas: +{amount}. Total: {currentLives}");
    }
    
    /// <summary>
    /// Retorna a quantidade atual de vidas
    /// </summary>
    public int GetCurrentLives()
    {
        return currentLives;
    }
    
    void GameOver()
    {
        Debug.Log("💀 GAME OVER! Todas as vidas foram perdidas!");
        OnGameOver?.Invoke();
        // Aqui pode adicionar lógica de game over (pausar jogo, mostrar tela, etc)
    }
    
    #endregion
    
    #region Wave Methods
    
    /// <summary>
    /// Avança para a próxima wave (incrementa contador)
    /// </summary>
    public void StartNextWave()
    {
        currentWave++;
        UpdateWaveUI();
        Debug.Log($"🌊 Wave {currentWave} iniciada!");
    }
    
    /// <summary>
    /// Chamado quando uma wave é completada (dá recompensa)
    /// </summary>
    public void CompleteWave()
    {
        // Dar recompensa de moedas pela wave completada
        AddMoney(moneyPerWave);
        Debug.Log($"✅ Wave {currentWave} completada! Recompensa: +{moneyPerWave} moedas");
    }
    
    /// <summary>
    /// Retorna o número da wave atual
    /// </summary>
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    #endregion
    
    #region Enemy Kill Rewards
    
    /// <summary>
    /// Chamado quando um inimigo normal morre
    /// </summary>
    public void OnNormalEnemyKilled()
    {
        AddMoney(normalEnemyReward);
        Debug.Log($"🎯 Inimigo normal eliminado! +{normalEnemyReward} moedas");
    }
    
    /// <summary>
    /// Chamado quando um inimigo voador morre
    /// </summary>
    public void OnFlyingEnemyKilled()
    {
        AddMoney(flyingEnemyReward);
        Debug.Log($"🎯 Inimigo voador eliminado! +{flyingEnemyReward} moedas");
    }
    
    /// <summary>
    /// Chamado quando um inimigo que se divide morre
    /// </summary>
    public void OnSplitEnemyKilled()
    {
        AddMoney(splitEnemyReward);
        Debug.Log($"🎯 Inimigo dividido eliminado! +{splitEnemyReward} moedas");
    }
    
    /// <summary>
    /// Método genérico para dar recompensa por matar inimigo
    /// </summary>
    public void OnEnemyKilled(int rewardAmount)
    {
        AddMoney(rewardAmount);
    }
    
    #endregion
    
    #region UI Update Methods
    
    /// <summary>
    /// Atualiza todos os textos da UI
    /// </summary>
    void UpdateAllUI()
    {
        UpdateMoneyUI();
        UpdateLivesUI();
        UpdateWaveUI();
    }
    
    /// <summary>
    /// Atualiza apenas o texto de moedas (formato: 4 dígitos)
    /// </summary>
    void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            // Formato com 4 casas (ex: 0500, 1234, 9999)
            moneyText.text = currentMoney.ToString("D4");
        }
    }
    
    /// <summary>
    /// Atualiza apenas o texto de vidas (formato: 2 dígitos)
    /// </summary>
    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            // Formato com 2 dígitos (ex: 15, 05, 00)
            livesText.text = currentLives.ToString("D2");
        }
    }
    
    /// <summary>
    /// Atualiza apenas o texto de wave (formato: 2 dígitos)
    /// </summary>
    void UpdateWaveUI()
    {
        if (waveText != null)
        {
            // Formato com 2 dígitos (ex: 01, 15, 99)
            waveText.text = currentWave.ToString("D2");
        }
    }
    
    #endregion
    
    #region Public Getters for Balancing
    
    /// <summary>
    /// Retorna a recompensa por matar inimigo normal (para balanceamento)
    /// </summary>
    public int GetNormalEnemyReward() => normalEnemyReward;
    
    /// <summary>
    /// Retorna a recompensa por matar inimigo voador (para balanceamento)
    /// </summary>
    public int GetFlyingEnemyReward() => flyingEnemyReward;
    
    /// <summary>
    /// Retorna a recompensa por matar inimigo dividido (para balanceamento)
    /// </summary>
    public int GetSplitEnemyReward() => splitEnemyReward;
    
    /// <summary>
    /// Retorna moedas ganhas por wave (para balanceamento)
    /// </summary>
    public int GetMoneyPerWave() => moneyPerWave;
    
    #endregion
}
