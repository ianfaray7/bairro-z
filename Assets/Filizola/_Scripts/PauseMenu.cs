using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Simple Pause Menu: pause on left mouse click when in a Map_* scene
public class PauseMenu : MonoBehaviour
{
    public string scenePrefix = "Map_"; // only enable pause in scenes that start with this prefix

    // Assign a prefab panel in inspector if you want; otherwise a simple runtime panel is created
    public GameObject pausePanelPrefab;

    private GameObject runtimePanel;
    private bool isPaused = false;
    private bool gameOver = false;
    private bool gameOverSubscribed = false;

    void Start()
    {
        // If not a map scene, disable this behaviour
        string name = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(name) || !name.StartsWith(scenePrefix))
        {
            enabled = false;
            return;
        }
    }
    
    void OnEnable()
    {
        PauseManager.OnPauseChanged += OnPauseChanged;
        TrySubscribeGameOver();
    }

    void OnDisable()
    {
        PauseManager.OnPauseChanged -= OnPauseChanged;
        if (ResourceManager.Instance != null && gameOverSubscribed)
        {
            ResourceManager.Instance.OnGameOver.RemoveListener(OnGameOver);
            gameOverSubscribed = false;
        }
    }

    private void TrySubscribeGameOver()
    {
        if (!gameOverSubscribed && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnGameOver.AddListener(OnGameOver);
            gameOverSubscribed = true;
            Debug.Log("PauseMenu: Subscribed to ResourceManager.OnGameOver");
        }
    }

    private void OnGameOver()
    {
        gameOver = true;
        Debug.Log("PauseMenu: OnGameOver called. Destroying pause UI if present.");
        if (runtimePanel != null)
        {
            Destroy(runtimePanel);
            runtimePanel = null;
        }
        // ensure we won't recreate the UI
        isPaused = false;
    }

    // Force hide for external callers (e.g., UIManager) when switching to Game Over
    public void ForceHidePauseUI()
    {
        gameOver = true;
        if (runtimePanel != null)
        {
            Debug.Log("PauseMenu: ForceHidePauseUI called. Destroying runtimePanel");
            Destroy(runtimePanel);
            runtimePanel = null;
        }
        isPaused = false;
    }

    public static void HideAllPauseUI()
    {
        var menus = GameObject.FindObjectsOfType<PauseMenu>();
        foreach (var m in menus)
        {
            m.ForceHidePauseUI();
        }
    }

    private void OnPauseChanged(bool paused)
    {
        Debug.Log($"PauseMenu: OnPauseChanged paused={paused}");
        isPaused = paused;

        // If the game is over, do not show pause UI over the death screen
        if (gameOver || (paused && (GameController.gameOver || (ResourceManager.Instance != null && ResourceManager.Instance.GetCurrentLives() <= 0))))
        {
            Debug.Log("PauseMenu: Game over detected - skipping pause UI creation");
            return;
        }

        if (runtimePanel != null)
        {
            runtimePanel.SetActive(paused);
            if (!paused)
            {
                Destroy(runtimePanel);
                runtimePanel = null;
            }
        }
        else if (paused)
        {
            runtimePanel = CreateRuntimePanel();
            Debug.Log("PauseMenu: runtime panel created in OnPauseChanged");
            runtimePanel.SetActive(true);
        }
    }
    
    void Update()
    {
        // Try to subscribe to game over event in case ResourceManager wasn't ready at OnEnable
        if (!gameOverSubscribed)
        {
            TrySubscribeGameOver();
        }
        if (!isPaused)
        {
            // left click to pause
            // use right click for pause to avoid conflict with shooting (left click)
            if (Input.GetMouseButtonDown(1))
            {
                // avoid pausing while clicking UI
                if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                // Avoid pause UI while in game over state
                if (gameOver || GameController.gameOver || (ResourceManager.Instance != null && ResourceManager.Instance.GetCurrentLives() <= 0))
                {
                    Debug.Log("PauseMenu: Right-click pause ignored because game is over or no lives.");
                }
                else
                {
                    PauseManager.Pause();
                    // Update our state and show panel
                    isPaused = true;
                    if (runtimePanel != null) runtimePanel.SetActive(true);
                }
            }
        }
        // ESC toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
                // If the game is over, ignore pause toggles to avoid conflict with death screen
                if (gameOver || GameController.gameOver || (ResourceManager.Instance != null && ResourceManager.Instance.GetCurrentLives() <= 0))
                {
                    Debug.Log("PauseMenu: ESC ignored because game is over");
                }
                else
                {
                    if (PauseManager.IsPaused) PauseManager.Resume();
                    else PauseManager.Pause();
                }

            // sync UI
            isPaused = PauseManager.IsPaused;
            if (runtimePanel != null) runtimePanel.SetActive(isPaused);
        }
    }

    public void Pause()
    {
        if (isPaused) return;

        // Do not show pause during game over
        if (GameController.gameOver || (ResourceManager.Instance != null && ResourceManager.Instance.GetCurrentLives() <= 0))
        {
            Debug.Log("PauseMenu: Pause requested but game over - skipping creating pause UI");
            PauseManager.Pause();
            isPaused = true;
            return;
        }

        // Create panel if needed
        if (pausePanelPrefab != null)
        {
            runtimePanel = Instantiate(pausePanelPrefab);
        }
        else
        {
            runtimePanel = CreateRuntimePanel();
        }

        if (runtimePanel != null) runtimePanel.SetActive(true);

        // freeze game by delegating to PauseManager
        PauseManager.Pause();
        isPaused = true;
        // Make sure we have an EventSystem so buttons are clickable
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    public void Resume()
    {
        if (!isPaused) return;

        // hide panel
        if (runtimePanel != null)
        {
            Destroy(runtimePanel);
            runtimePanel = null;
        }

        // unfreeze
        // PauseManager will handle resume logic
        PauseManager.Resume();
        // note: PauseManager fires OnPauseChanged; we still cleanup panel
        isPaused = PauseManager.IsPaused;
    }

    public void ReturnToMenu()
    {
        // unpause and load main menu (index 0)
        AudioListener.pause = false;
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(0);
    }

    // Build a simple panel in runtime with two buttons: Continue and Main Menu
    private GameObject CreateRuntimePanel()
    {
        LogCanvasDiagnostics();

        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        // Always create a dedicated canvas for the pause panel to ensure it is visible on top
        Canvas hostCanvas = null;
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hostCanvas = canvas;
        }
        else
        {
            // create a dedicated overlay canvas so pause UI is always visible (do not modify others)
            var pauseCanvasGO = new GameObject("PauseCanvas");
            hostCanvas = pauseCanvasGO.AddComponent<Canvas>();
            pauseCanvasGO.AddComponent<CanvasScaler>();
            pauseCanvasGO.AddComponent<GraphicRaycaster>();
            hostCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            try { hostCanvas.overrideSorting = true; hostCanvas.sortingOrder = 20000; } catch { }
        }

        // If we created a host canvas, use it — otherwise fall back to found canvas
        if (hostCanvas != null)
        {
            canvas = hostCanvas;
        }

        Debug.Log($"PauseMenu: Using canvas '{canvas.gameObject.name}' (overrideSorting={canvas.overrideSorting}, sortingOrder={canvas.sortingOrder}, active={canvas.gameObject.activeInHierarchy})");

        var panel = new GameObject("PausePanel");
        panel.transform.SetParent(canvas.transform, false);
        Debug.Log($"PauseMenu: Panel set parent='{panel.transform.parent?.name}' active={panel.activeSelf} scale={panel.transform.localScale}");
        Debug.Log($"PauseMenu: Created panel '{panel.name}' under canvas '{canvas.gameObject.name}'. Panel active: {panel.activeSelf}");
        panel.transform.SetAsLastSibling();
        var img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.6f);
        img.raycastTarget = true; // block clicks to underneath elements
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Container
        var container = new GameObject("Container");
        container.transform.SetParent(panel.transform, false);
        var cRect = container.AddComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(400, 220);
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.anchoredPosition = Vector2.zero;

        var layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        var csf = container.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Title text
        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(container.transform, false);
        var title = titleGo.AddComponent<Text>();
        title.text = "PAUSADO";
        title.alignment = TextAnchor.MiddleCenter;
        title.fontSize = 36;
        title.color = Color.white;
        Font tf = null;
        try
        {
            // LegacyRuntime.ttf é a fonte interna recomendada para versões recentes do Unity
            tf = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch { tf = null; }
        if (tf == null)
        {
            try { tf = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { tf = null; }
        }
        if (tf != null) title.font = tf;
        // add outline for readability
        var titleOutline = title.gameObject.AddComponent<UnityEngine.UI.Outline>();
        titleOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        titleOutline.effectDistance = new Vector2(2f, -2f);

        // Try to reuse MapSelectionUI border sprite to style buttons like selection
        Sprite borderSprite = null;
        var ms = UnityEngine.Object.FindFirstObjectByType<MapSelectionUI>();
        if (ms != null) borderSprite = ms.buttonBorderSprite;
    #if UNITY_EDITOR
        if (borderSprite == null)
        {
            var defaultPath = "Assets/Ian/Gameplay Hud/Upgrade_bubble.png";
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(defaultPath);
            if (sp != null) borderSprite = sp;
        }
    #endif

        // Continue Button (styled)
        var continueStyled = CreateStyledButton(container.transform, "Continuar", borderSprite, Color.white);
        Debug.Log($"PauseMenu: Continue button Path: {GetHierarchyPath(continueStyled.gameObject)}");
        Debug.Log("PauseMenu: Continue button created");
        continueStyled.onClick.AddListener(() => Resume());

        // Return to menu (styled)
        var returnStyled = CreateStyledButton(container.transform, "Voltar ao Menu", borderSprite, Color.white);
        Debug.Log($"PauseMenu: Return button Path: {GetHierarchyPath(returnStyled.gameObject)}");
        Debug.Log("PauseMenu: Return button created");
        returnStyled.onClick.AddListener(() => ReturnToMenu());

        return panel;
    }

    private Button CreateButton(Transform parent, string text)
    {
        var btnGo = new GameObject("Button_");
        btnGo.transform.SetParent(parent, false);
        var rt = btnGo.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 50);
        var img = btnGo.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        var btn = btnGo.AddComponent<Button>();

        var tGo = new GameObject("Text");
        tGo.transform.SetParent(btnGo.transform, false);
        var txt = tGo.AddComponent<Text>();
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        Font tf = null;
        try
        {
            tf = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch { tf = null; }
        if (tf == null)
        {
            try { tf = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { tf = null; }
        }
        if (tf != null) txt.font = tf;
        txt.fontSize = 20;

        var layoutEl = btnGo.AddComponent<LayoutElement>();
        layoutEl.preferredWidth = 300;
        layoutEl.preferredHeight = 50;

        return btn;
    }

    // Create a styled button similar to MapSelectionUI
    private Button CreateStyledButton(Transform parent, string text, Sprite borderSprite = null, Color textColor = default)
    {
        var btn = CreateButton(parent, text);

        var bkg = btn.GetComponent<UnityEngine.UI.Image>();
        // Apply border sprite if available
        if (borderSprite != null && bkg != null)
        {
            bkg.sprite = borderSprite;
            bkg.type = Image.Type.Sliced;
            // if text is white, use darker background for better contrast
            bkg.color = (textColor == default || textColor == Color.black) ? Color.white : new Color(0.15f, 0.15f, 0.15f, 1f);

            // attempt to resize to sprite rect to get look similar to selection
            var layout = btn.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layout == null) layout = btn.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            layout.preferredWidth = Mathf.RoundToInt(borderSprite.rect.width);
            layout.preferredHeight = Mathf.RoundToInt(borderSprite.rect.height);
        }

        // Text centering and color (match MapSelectionUI - black text)
        var textComp = btn.GetComponentInChildren<UnityEngine.UI.Text>();
        if (textComp != null)
        {
            textComp.color = textColor == default ? Color.black : textColor;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.rectTransform.anchorMin = Vector2.zero;
            textComp.rectTransform.anchorMax = Vector2.one;
            textComp.rectTransform.offsetMin = Vector2.zero;
            textComp.rectTransform.offsetMax = Vector2.zero;
            // Add outline for contrast if text is white
            if (textComp.color == Color.white && textComp.GetComponent<UnityEngine.UI.Outline>() == null)
            {
                var ol = textComp.gameObject.AddComponent<UnityEngine.UI.Outline>();
                ol.effectColor = new Color(0f, 0f, 0f, 0.8f);
                ol.effectDistance = new Vector2(1.5f, -1.5f);
            }
        }

        var tmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.color = textColor == default ? Color.black : textColor;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.rectTransform.anchorMin = Vector2.zero;
            tmp.rectTransform.anchorMax = Vector2.one;
            tmp.rectTransform.offsetMin = Vector2.zero;
            tmp.rectTransform.offsetMax = Vector2.zero;
            // Add outline/shadow for TMP if using white text
            if (tmp.color == Color.white && tmp.GetComponent<UnityEngine.UI.Shadow>() == null)
            {
                var shadow = tmp.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
                shadow.effectDistance = new Vector2(1.2f, -1.2f);
            }
        }

        return btn;
    }

    private string GetHierarchyPath(GameObject go)
    {
        if (go == null) return "null";
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }

    private void LogCanvasDiagnostics()
    {
        var canvases = GameObject.FindObjectsOfType<Canvas>();
        Debug.Log($"PauseMenu: Found {canvases.Length} canvases in scene:");
        foreach (var c in canvases)
        {
            Debug.Log($" - {GetHierarchyPath(c.gameObject)} renderMode={c.renderMode} overrideSorting={c.overrideSorting} sortingOrder={c.sortingOrder} active={c.gameObject.activeInHierarchy}");
        }

        var es = UnityEngine.EventSystems.EventSystem.current;
        Debug.Log($"PauseMenu: EventSystem present: {es != null}");
    }
}
