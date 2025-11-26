using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Joystick virtual para controles mobile
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    
    [Header("Settings")]
    [SerializeField] private float handleRange = 50f;
    [SerializeField] private bool dynamicJoystick = false; // Se true, joystick aparece onde tocar
    
    private Vector2 input = Vector2.zero;
    private Vector2 joystickCenter;
    private Canvas canvas;
    private Camera cam;
    
    public static VirtualJoystick Instance { get; private set; }
    
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
            Debug.LogError("VirtualJoystick precisa estar dentro de um Canvas!");
        }
        
        // Pega camera do canvas
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            cam = canvas.worldCamera;
        }
        
        // Salva posição inicial
        if (background != null)
        {
            joystickCenter = background.position;
        }
        
        // Esconde o joystick em plataformas não-mobile
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (dynamicJoystick && background != null)
        {
            // Move o joystick para onde tocou
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                eventData.position, 
                cam, 
                out position
            );
            background.anchoredPosition = position;
            joystickCenter = background.position;
        }
        
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
        
        position = Vector2.ClampMagnitude(position, handleRange);
        
        if (handle != null)
        {
            handle.anchoredPosition = position;
        }
        
        // Normaliza o input
        input = position / handleRange;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
        
        // Se for joystick dinâmico, volta pra posição original
        if (dynamicJoystick && background != null)
        {
            background.position = joystickCenter;
        }
    }
    
    /// <summary>
    /// Retorna o input do joystick (valores entre -1 e 1)
    /// </summary>
    public Vector2 GetInput()
    {
        return input;
    }
    
    /// <summary>
    /// Retorna input horizontal (-1 a 1)
    /// </summary>
    public float Horizontal()
    {
        return input.x;
    }
    
    /// <summary>
    /// Retorna input vertical (-1 a 1)
    /// </summary>
    public float Vertical()
    {
        return input.y;
    }
}
