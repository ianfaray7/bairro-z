using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject endGamePanel;
    private bool endShown = false;
    private bool gameOverSubscribed = false;
    void Start()
    {
        Debug.Log("UIManager: Start called");
        if (endGamePanel != null) endGamePanel.SetActive(false);
        // Subscribe to resource manager game-over event if available
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnGameOver.AddListener(ShowEndGamePanel);
            gameOverSubscribed = true;
            Debug.Log("UIManager: subscribed to ResourceManager.OnGameOver");
        }
        else
        {
            Debug.LogWarning("UIManager: ResourceManager.Instance not found. GameOver must be handled by GameController or set manually.");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Fallback: if GameController signals game over (collectibles reached 0), show end panel
        if (!endShown && GameController.gameOver)
        {
            ShowEndGamePanel();
        }

        // If ResourceManager was created after this object, subscribe once when it's ready
        if (!gameOverSubscribed && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnGameOver.AddListener(ShowEndGamePanel);
            gameOverSubscribed = true;
            Debug.Log("UIManager: deferred subscription to ResourceManager.OnGameOver");
        }

        // Extra fallback: if our resource manager reports 0 lives, show end panel
        if (!endShown && ResourceManager.Instance != null)
        {
            try
            {
                if (ResourceManager.Instance.GetCurrentLives() <= 0)
                {
                    Debug.Log("UIManager: ResourceManager reports 0 lives, calling ShowEndGamePanel");
                    ShowEndGamePanel();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("UIManager: Error checking ResourceManager lives: " + ex.Message);
            }
        }
    }

    private void ShowEndGamePanel()
    {
        Debug.Log("UIManager: ShowEndGamePanel called");
        if (endShown) return;
        endShown = true;

        // Make sure any pause UI is hidden (defensive)
        PauseMenu.HideAllPauseUI();

        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);
            PauseManager.Pause();
            // Hide potential pause UI that could be created during Pause() callbacks
            PauseMenu.HideAllPauseUI();
            // wire first found button to return to menu
            var btn = endGamePanel.GetComponentInChildren<Button>(true);
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => { PauseManager.Resume(); UnityEngine.SceneManagement.SceneManager.LoadScene(0); });
            }
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
        else
        {
            CreateRuntimeDeathPanel();
        }
    }

    private void CreateRuntimeDeathPanel()
    {
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas_Death");
            canvas = cgo.AddComponent<Canvas>();
            cgo.AddComponent<UnityEngine.UI.CanvasScaler>();
            cgo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 30000;
        }

        var panel = new GameObject("DeathPanel");
        panel.transform.SetParent(canvas.transform, false);
        var img = panel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0f, 0f, 0f, 0.8f);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

        var container = new GameObject("Container");
        container.transform.SetParent(panel.transform, false);
        var crect = container.AddComponent<RectTransform>();
        crect.sizeDelta = new Vector2(420, 200);
        crect.anchorMin = new Vector2(0.5f,0.5f); crect.anchorMax = crect.anchorMin; crect.anchoredPosition = Vector2.zero;

        var layout = container.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter; layout.spacing = 8f;

        var titleGo = new GameObject("Title"); titleGo.transform.SetParent(container.transform, false);
        var title = titleGo.AddComponent<UnityEngine.UI.Text>(); title.text = "Você perdeu"; title.alignment = TextAnchor.MiddleCenter; title.color = Color.white; title.fontSize = 36;
        var fnt = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Font>("LegacyRuntime.ttf"); if (fnt == null) fnt = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Font>("Arial.ttf"); if (fnt != null) title.font = fnt;

        var btnGo = new GameObject("ReturnToMenu"); btnGo.transform.SetParent(container.transform,false);
        var bimg = btnGo.AddComponent<UnityEngine.UI.Image>(); bimg.color = new Color(0.2f,0.2f,0.2f,1f);
        var btn = btnGo.AddComponent<UnityEngine.UI.Button>();
        var t = new GameObject("Text"); t.transform.SetParent(btnGo.transform,false);
        var tt = t.AddComponent<UnityEngine.UI.Text>(); tt.text = "Voltar ao Menu"; tt.alignment = TextAnchor.MiddleCenter; tt.color = Color.white; tt.fontSize=20; if (fnt!=null) tt.font = fnt;
        var le = btnGo.AddComponent<UnityEngine.UI.LayoutElement>(); le.preferredWidth = 300; le.preferredHeight = 50;
        btn.onClick.AddListener(() => { PauseManager.Resume(); UnityEngine.SceneManagement.SceneManager.LoadScene(0); });

        PauseManager.Pause();
    }
}
