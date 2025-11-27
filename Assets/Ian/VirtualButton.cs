using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Botão virtual para ações (ataque, interação, etc)
/// </summary>
public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    [SerializeField] private string buttonName = "Attack";
    
    private bool isPressed = false;
    private bool wasPressed = false;
    
    public static VirtualButton AttackButton { get; private set; }
    
    void Awake()
    {
        // Registra botão de ataque
        if (buttonName == "Attack")
        {
            AttackButton = this;
        }
        
        // Esconde botões em plataformas não-mobile
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
    
    void LateUpdate()
    {
        wasPressed = false;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        wasPressed = true;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
    
    /// <summary>
    /// Retorna true enquanto o botão está pressionado
    /// </summary>
    public bool GetButton()
    {
        return isPressed;
    }
    
    /// <summary>
    /// Retorna true no frame que o botão foi pressionado
    /// </summary>
    public bool GetButtonDown()
    {
        return wasPressed;
    }
}
