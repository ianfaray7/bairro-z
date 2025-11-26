using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Joystick de mira e ataque - controla direção de ataque
/// </summary>
public class AttackJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    
    [Header("Settings")]
    [SerializeField] private float handleRange = 50f;
    [SerializeField] private float attackThreshold = 0.3f; // Mínimo para começar a atirar
    
    private Vector2 input = Vector2.zero;
    private bool isAttacking = false;
    private bool isTouching = false; // Rastreia se está tocando o joystick
    private Canvas canvas;
    private Camera cam;
    
    public static AttackJoystick Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("AttackJoystick precisa estar dentro de um Canvas!");
        }
        
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            cam = canvas.worldCamera;
        }
        
        // Esconde em plataformas não-mobile
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isTouching = true;
        OnDrag(eventData);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, 
            eventData.position, 
            cam, 
            out position
        );
        
        // 'position' agora é a posição relativa ao centro do joystick
        // Limita visualmente o handle
        Vector2 clampedPosition = Vector2.ClampMagnitude(position, handleRange);
        
        if (handle != null)
        {
            handle.anchoredPosition = clampedPosition;
        }
        
        // Calcula o input baseado na posição relativa ao centro
        // Normaliza para valores entre -1 e 1
        input = position / handleRange;
        
        // Limita a magnitude máxima em 1 (mas mantém a direção)
        if (input.magnitude > 1f)
        {
            input = input.normalized;
        }
        
        // Determina se está atacando baseado na magnitude
        isAttacking = input.magnitude >= attackThreshold;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isTouching = false;
        input = Vector2.zero;
        isAttacking = false;
        
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }
    
    /// <summary>
    /// Retorna true se está tocando o joystick
    /// </summary>
    public bool IsTouching()
    {
        return isTouching;
    }
    
    /// <summary>
    /// Retorna a direção de mira normalizada
    /// </summary>
    public Vector2 GetAimDirection()
    {
        return input.normalized;
    }
    
    /// <summary>
    /// Retorna true se está atacando (joystick movido além do threshold)
    /// </summary>
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    /// <summary>
    /// Retorna o input bruto do joystick
    /// </summary>
    public Vector2 GetInput()
    {
        return input;
    }
}
