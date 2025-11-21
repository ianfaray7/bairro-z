using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Gerencia pop-ups informativos de wave no centro da tela
/// </summary>
public class WavePopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    
    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private bool useScale = true;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine currentPopupCoroutine;
    
    // Singleton
    public static WavePopup Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ WavePopup Instance criado!");
        }
        else
        {
            Debug.LogWarning("⚠️ WavePopup duplicado! Destruindo...");
            Destroy(gameObject);
        }
        
        // Pega componentes
        if (popupPanel != null)
        {
            canvasGroup = popupPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = popupPanel.AddComponent<CanvasGroup>();
            }
            
            rectTransform = popupPanel.GetComponent<RectTransform>();
        }
    }
    
    void Start()
    {
        HidePopup();
    }
    
    /// <summary>
    /// Mostra popup de wave iniciando
    /// </summary>
    public void ShowWaveStart(int waveNumber, float timeUntilStart)
    {
        string message = $"Wave {waveNumber:D2} inicia em {Mathf.CeilToInt(timeUntilStart)} segundos!";
        ShowPopup(message);
    }
    
    /// <summary>
    /// Mostra popup de wave iniciando (sem countdown)
    /// </summary>
    public void ShowWaveStartNow(int waveNumber)
    {
        string message = $"Wave {waveNumber:D2} iniciando!";
        ShowPopup(message);
    }
    
    /// <summary>
    /// Mostra popup de wave completa
    /// </summary>
    public void ShowWaveComplete(int waveNumber)
    {
        string message = $"Wave {waveNumber:D2} concluída! +150 moedas!";
        ShowPopup(message);
    }
    
    /// <summary>
    /// Mostra popup genérico
    /// </summary>
    public void ShowPopup(string message)
    {
        if (popupPanel == null || popupText == null)
        {
            Debug.LogError("❌ WavePopup: Panel ou Text não configurado!");
            return;
        }
        
        // Para o popup anterior se houver
        if (currentPopupCoroutine != null)
        {
            StopCoroutine(currentPopupCoroutine);
        }
        
        currentPopupCoroutine = StartCoroutine(ShowPopupCoroutine(message));
    }
    
    IEnumerator ShowPopupCoroutine(string message)
    {
        // Configura texto
        popupText.text = message;
        
        // Mostra painel
        popupPanel.SetActive(true);
        
        // Fade In
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = t;
            }
            
            if (useScale && rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            }
            
            yield return null;
        }
        
        // Garante valores finais
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (useScale && rectTransform != null) rectTransform.localScale = Vector3.one;
        
        // Aguarda tempo de display
        yield return new WaitForSeconds(displayDuration);
        
        // Fade Out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - t;
            }
            
            if (useScale && rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            }
            
            yield return null;
        }
        
        // Esconde
        HidePopup();
    }
    
    void HidePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.zero;
        }
    }
    
    /// <summary>
    /// Mostra popup de preparação inicial
    /// </summary>
    public void ShowPreparation()
    {
        ShowPopup("Se prepare! Os zumbis estão vindo!");
    }
    
    /// <summary>
    /// Mostra popup de vitória
    /// </summary>
    public void ShowVictory()
    {
        ShowPopup("VITÓRIA! Todas as waves concluídas!");
    }
    
    /// <summary>
    /// Mostra popup de derrota
    /// </summary>
    public void ShowGameOver()
    {
        ShowPopup("GAME OVER!");
    }
}
