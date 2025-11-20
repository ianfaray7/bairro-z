using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gerencia o timer visual de intervalo entre waves
/// Mostra contagem regressiva até a próxima wave
/// </summary>
public class WaveTimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFillImage; // Opcional: barra circular de progresso
    [SerializeField] private GameObject timerPanel; // Painel que contém o timer
    
    [Header("Timer Settings")]
    [SerializeField] private float timeBetweenWaves = 30f; // 30 segundos entre waves
    [SerializeField] private bool startTimerOnStart = false;
    
    private float currentTimer;
    private bool isTimerRunning = false;
    private bool isWaveActive = false;
    
    // Singleton
    public static WaveTimerUI Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ WaveTimerUI Instance criado!");
        }
        else
        {
            Debug.LogWarning("⚠️ WaveTimerUI duplicado! Destruindo...");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        Debug.Log($"⏰ WaveTimerUI Start - Timer Text: {(timerText != null ? "OK" : "NULL")}, Timer Panel: {(timerPanel != null ? "OK" : "NULL")}, Fill Image: {(timerFillImage != null ? "OK" : "NULL")}");
        
        if (startTimerOnStart)
        {
            StartTimer();
        }
        else
        {
            HideTimer();
        }
    }
    
    void Update()
    {
        if (isTimerRunning && !isWaveActive)
        {
            currentTimer -= Time.deltaTime;
            
            if (currentTimer <= 0)
            {
                currentTimer = 0;
                OnTimerComplete();
            }
            
            UpdateTimerUI();
        }
    }
    
    /// <summary>
    /// Inicia a contagem regressiva para próxima wave
    /// </summary>
    public void StartTimer()
    {
        currentTimer = timeBetweenWaves;
        isTimerRunning = true;
        isWaveActive = false;
        ShowTimer();
        UpdateTimerUI();
        Debug.Log($"⏰ Timer iniciado: {timeBetweenWaves} segundos até próxima wave");
    }
    
    /// <summary>
    /// Para o timer (chamado quando wave começa)
    /// </summary>
    public void StopTimer()
    {
        isTimerRunning = false;
        isWaveActive = true;
        HideTimer();
    }
    
    /// <summary>
    /// Inicia uma wave manualmente (botão ou auto)
    /// </summary>
    public void StartWaveNow()
    {
        if (!isWaveActive)
        {
            currentTimer = 0;
            
            // 🌊 Chama WaveManager imediatamente
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.StartNextWave();
            }
            else
            {
                OnTimerComplete();
            }
        }
    }
    
    /// <summary>
    /// Chamado quando o timer chega a 0
    /// </summary>
    void OnTimerComplete()
    {
        isTimerRunning = false;
        isWaveActive = true;
        
        // 🌊 Chama WaveManager para iniciar a próxima wave
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartNextWave();
        }
        else
        {
            // Fallback: se não houver WaveManager, notifica ResourceManager
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.StartNextWave();
            }
        }
        
        Debug.Log("⏰ Timer completo! Iniciando próxima wave...");
        HideTimer();
    }
    
    /// <summary>
    /// Chamado quando a wave termina para reiniciar o timer
    /// </summary>
    public void OnWaveComplete()
    {
        Debug.Log("⏰ OnWaveComplete chamado! Reiniciando timer...");
        isWaveActive = false;
        StartTimer();
    }
    
    /// <summary>
    /// Atualiza o texto e imagem do timer
    /// </summary>
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Formato MM:SS
            int minutes = Mathf.FloorToInt(currentTimer / 60);
            int seconds = Mathf.FloorToInt(currentTimer % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        
        // Atualiza barra de progresso circular (se houver)
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = currentTimer / timeBetweenWaves;
        }
    }
    
    void ShowTimer()
    {
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
            Debug.Log("⏰ Timer panel MOSTRADO (SetActive = true)");
        }
        else
        {
            Debug.LogError("❌ Timer Panel é NULL! Arraste o GameObject no Inspector.");
        }
    }
    
    void HideTimer()
    {
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
            Debug.Log("⏰ Timer panel ESCONDIDO (SetActive = false)");
        }
    }
    
    #region Public Getters/Setters
    
    public float GetTimeBetweenWaves() => timeBetweenWaves;
    
    public void SetTimeBetweenWaves(float time)
    {
        timeBetweenWaves = time;
    }
    
    public bool IsWaveActive() => isWaveActive;
    
    public float GetCurrentTimer() => currentTimer;
    
    #endregion
}
