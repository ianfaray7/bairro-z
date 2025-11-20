using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    
    [Header("Optional Color Settings")]
    public bool changeColorByHealth = true;
    public Color fullHealthColor = Color.green;
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    
    private Image fillImage;
    
    void Start()
    {
        if (healthBar != null)
        {
            fillImage = healthBar.fillRect.GetComponent<Image>();
        }
        
        // Registra evento de mudança de vida
        if (BaseHealth.instance != null)
        {
            BaseHealth.instance.OnHealthChanged += UpdateUI;
            BaseHealth.instance.OnBaseDestroyed += OnBaseDestroyed;
            
            // Atualiza UI inicial
            UpdateUI(BaseHealth.instance.currentHealth, BaseHealth.instance.maxHealth);
        }
        else
        {
            Debug.LogWarning("BaseHealth.instance não encontrado! Certifique-se que existe um GameObject com BaseHealth na cena.");
        }
    }
    
    void UpdateUI(float currentHealth, float maxHealth)
    {
        // Atualiza barra
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            
            // Muda cor se configurado
            if (changeColorByHealth && fillImage != null)
            {
                float percentage = currentHealth / maxHealth;
                
                if (percentage > 0.5f)
                {
                    fillImage.color = Color.Lerp(halfHealthColor, fullHealthColor, (percentage - 0.5f) * 2f);
                }
                else
                {
                    fillImage.color = Color.Lerp(lowHealthColor, halfHealthColor, percentage * 2f);
                }
            }
        }
        
        // Atualiza texto
        if (healthText != null)
        {
            healthText.text = $"Base: {currentHealth:F0}/{maxHealth:F0}";
        }
    }
    
    void OnBaseDestroyed()
    {
        if (healthText != null)
        {
            healthText.text = "BASE DESTRUÍDA!";
            healthText.color = Color.red;
        }
        
        Debug.Log("Game Over - Base destruída!");
        // Aqui você pode adicionar lógica de game over, pausar o jogo, etc.
    }
    
    void OnDestroy()
    {
        // Desregistra eventos
        if (BaseHealth.instance != null)
        {
            BaseHealth.instance.OnHealthChanged -= UpdateUI;
            BaseHealth.instance.OnBaseDestroyed -= OnBaseDestroyed;
        }
    }
}
