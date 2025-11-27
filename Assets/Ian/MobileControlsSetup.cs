using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cria controles mobile automaticamente na cena
/// Adicione este script em um GameObject vazio nas suas cenas de jogo
/// </summary>
public class MobileControlsSetup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool createOnStart = true;
    [SerializeField] private Color joystickColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color buttonColor = new Color(1f, 0.2f, 0.2f, 0.5f);
    
    private GameObject mobileUI;
    
    void Start()
    {
        // Habilita multi-touch para usar os dois joysticks ao mesmo tempo
        Input.multiTouchEnabled = true;
        
        if (createOnStart)
        {
            CreateMobileControls();
        }
    }
    
    public void CreateMobileControls()
    {
        // Só cria em mobile ou no editor (para testar)
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
        return;
#endif
        
        // Verifica se já existe
        if (mobileUI != null || GameObject.Find("MobileControls_Canvas") != null)
        {
            Debug.Log("Controles mobile já existem na cena.");
            return;
        }
        
        // Cria Canvas
        mobileUI = new GameObject("MobileControls_Canvas");
        Canvas canvas = mobileUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = mobileUI.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        mobileUI.AddComponent<GraphicRaycaster>();
        
        // Cria Joystick
        CreateJoystick(canvas.transform);
        
        // Cria Joystick de Ataque (direita)
        CreateAttackJoystick(canvas.transform);
        
        // Cria Botão de Pause (canto superior direito)
        CreatePauseButton(canvas.transform);
        
        Debug.Log("✅ Controles mobile criados com sucesso!");
    }
    
    private void CreateJoystick(Transform parent)
    {
        // Container do Joystick
        GameObject joystickObj = new GameObject("Joystick");
        joystickObj.transform.SetParent(parent, false);
        RectTransform joystickRect = joystickObj.AddComponent<RectTransform>();
        joystickRect.sizeDelta = new Vector2(200, 200);
        joystickRect.anchorMin = new Vector2(0, 0);
        joystickRect.anchorMax = new Vector2(0, 0);
        joystickRect.pivot = new Vector2(0.5f, 0.5f);
        joystickRect.anchoredPosition = new Vector2(150, 150);
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(joystickObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(200, 200);
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = joystickColor;
        bgImage.sprite = CreateCircleSprite();
        
        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(bg.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(80, 80);
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(joystickColor.r, joystickColor.g, joystickColor.b, joystickColor.a * 2f);
        handleImage.sprite = CreateCircleSprite();
        
        // Adiciona script
        VirtualJoystick joystick = joystickObj.AddComponent<VirtualJoystick>();
        
        // Usa reflection para setar os campos privados
        var bgField = typeof(VirtualJoystick).GetField("background", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var handleField = typeof(VirtualJoystick).GetField("handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rangeField = typeof(VirtualJoystick).GetField("handleRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (bgField != null) bgField.SetValue(joystick, bgRect);
        if (handleField != null) handleField.SetValue(joystick, handleRect);
        if (rangeField != null) rangeField.SetValue(joystick, 50f);
    }
    
    private void CreateAttackJoystick(Transform parent)
    {
        // Container do Joystick de Ataque
        GameObject joystickObj = new GameObject("AttackJoystick");
        joystickObj.transform.SetParent(parent, false);
        RectTransform joystickRect = joystickObj.AddComponent<RectTransform>();
        joystickRect.sizeDelta = new Vector2(200, 200);
        joystickRect.anchorMin = new Vector2(1, 0);
        joystickRect.anchorMax = new Vector2(1, 0);
        joystickRect.pivot = new Vector2(0.5f, 0.5f);
        joystickRect.anchoredPosition = new Vector2(-150, 150);
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(joystickObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(200, 200);
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = buttonColor;
        bgImage.sprite = CreateCircleSprite();
        
        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(bg.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(80, 80);
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(buttonColor.r, buttonColor.g, buttonColor.b, buttonColor.a * 2f);
        handleImage.sprite = CreateCircleSprite();
        
        // Texto "AIM"
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bg.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = Vector2.zero;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        
        Text text = textObj.AddComponent<Text>();
        text.text = "AIM";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 1f, 1f, 0.5f);
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        // Adiciona script
        AttackJoystick attackJoystick = joystickObj.AddComponent<AttackJoystick>();
        
        // Usa reflection para setar os campos privados
        var bgField = typeof(AttackJoystick).GetField("background", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var handleField = typeof(AttackJoystick).GetField("handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rangeField = typeof(AttackJoystick).GetField("handleRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var thresholdField = typeof(AttackJoystick).GetField("attackThreshold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (bgField != null) bgField.SetValue(attackJoystick, bgRect);
        if (handleField != null) handleField.SetValue(attackJoystick, handleRect);
        if (rangeField != null) rangeField.SetValue(attackJoystick, 50f);
        if (thresholdField != null) thresholdField.SetValue(attackJoystick, 0.3f);
    }
    
    private void CreatePauseButton(Transform parent)
    {
        // Botão de Pause
        GameObject btnObj = new GameObject("PauseButton");
        btnObj.transform.SetParent(parent, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(80, 80);
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-20, -20);
        
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        btnImage.sprite = CreateCircleSprite();
        
        // Texto "||"
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = Vector2.zero;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        
        Text text = textObj.AddComponent<Text>();
        text.text = "||";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = 32;
        text.fontStyle = FontStyle.Bold;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        // Adiciona Button component
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            if (PauseManager.IsPaused)
                PauseManager.Resume();
            else
                PauseManager.Pause();
        });
    }
    
    private Sprite CreateCircleSprite()
    {
        // Cria um sprite circular simples
        Texture2D texture = new Texture2D(256, 256);
        Color[] pixels = new Color[256 * 256];
        
        Vector2 center = new Vector2(128, 128);
        float radius = 128;
        
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * 256 + x] = distance <= radius ? Color.white : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
    }
}
