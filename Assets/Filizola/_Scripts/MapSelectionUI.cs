using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Popula um painel com botões para cada cena do tipo "Map_*" definida no Build Settings
/// - Arraste um prefab de Button em `buttonPrefab` (um Button com um filho Text/TextMeshPro)
/// - `contentParent` é o Transform onde os botões serão instanciados
/// - `panel` é o GameObject que será habilitado/desabilitado como tela de seleção
/// </summary>
public class MapSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public Transform contentParent; // onde os botões são instanciados
    public Button buttonPrefab;

    [Header("Filters & Settings")]
    public string scenePrefix = "Map_"; // lista apenas cenas que começam com esse prefixo
    [Header("Styling")]
    public Color buttonColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color buttonHoverColor = new Color(0.8f, 0.8f, 1f, 1f);
    public Vector2 buttonSize = new Vector2(360, 80);
    // procura miniaturas em Resources/MapThumbnails/{sceneName}
    public string thumbnailResourcePath = "MapThumbnails";
    [Header("Debug / Visibility")]
    public bool forceVisibleButtons = true; // se true, garante bkg.color != transparent para MapList fallback e runtime
    [Header("Fallback Options")]
    public MapList mapList; // optional ScriptableObject in Resources or assigned in Inspector
    [Header("Layout Options")]
    public bool useHorizontalLayout = true;
    public Sprite buttonBorderSprite; // arraste aqui o 'Artboard 5' do Inspector
    public bool useBorderSpriteSize = true; // se true, o botão terá o tamanho do sprite de borda
    [Header("Editor Tools")]
    [Tooltip("Quando habilitado no Editor, força o MapList fallback como se fosse WebGL — útil para testar sem builds.")]
    public bool simulateWebGL = false;

    private List<string> scenes = new List<string>();
    // map scene name -> path (Assets/..../xxx.unity)
    private System.Collections.Generic.Dictionary<string, string> scenePaths = new System.Collections.Generic.Dictionary<string, string>();

    // Map short scene names to display names
    private string MapDisplayName(string shortName)
    {
        if (string.IsNullOrEmpty(shortName)) return shortName;
        // remove prefix if any (Map_)
        var baseName = shortName.Replace(scenePrefix, "").ToLower();
        switch (baseName)
        {
            case "first": return "Primeira fase";
            case "div": return "Segunda fase";
            case "split": return "BOSS";
            default: return shortName.Replace(scenePrefix, "").Replace('_', ' ');
        }
    }

    void Start()
    {
        // If a border sprite isn't explicitly assigned in the Inspector, try a runtime Resources lookup.
        // This helps show a non-empty border in builds (WebGL) when someone put the art into a Resources folder.
        if (buttonBorderSprite == null)
        {
            // try a common fallback name (put the sprite into Assets/Resources/Upgrade_bubble.png)
            var tryRes = Resources.Load<Sprite>("Upgrade_bubble");
            if (tryRes != null)
            {
                buttonBorderSprite = tryRes;
                Debug.Log("MapSelectionUI: Loaded fallback 'Upgrade_bubble' from Resources at runtime.");
            }
        }
        #if UNITY_EDITOR
        // Auto-assign artboard sprite if not set (editor only) to speed up testing
        if (buttonBorderSprite == null)
        {
            var defaultPath = "Assets/Ian/Gameplay Hud/Upgrade_bubble.png";
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(defaultPath);
            if (sp != null) buttonBorderSprite = sp;
        }
        #endif
        if (panel == null || contentParent == null || buttonPrefab == null)
        {
            Debug.LogWarning("MapSelectionUI: Configure panel, contentParent e buttonPrefab no Inspector.");
            return;
        }

        PopulateList();
    }

    public void PopulateList()
    {
        // limpa antigos
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        scenes.Clear();

        bool forceMapListFromEditor = Application.isEditor && simulateWebGL;
        int count = 0;
        if (!forceMapListFromEditor) count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(name)) continue;

            if (!string.IsNullOrEmpty(scenePrefix))
            {
                if (!name.StartsWith(scenePrefix)) continue;
            }

            scenes.Add(name);
            // store path used so we can auto-add to build settings if needed
            if (!scenePaths.ContainsKey(name)) scenePaths[name] = path;

            var b = Instantiate(buttonPrefab, contentParent);
            b.name = "MapButton_" + name;
            // find label components (Text or TextMeshPro) and set display name
            var label = b.GetComponentInChildren<Text>();
            var labelTMP = b.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = MapDisplayName(name);
                label.gameObject.SetActive(true);
                label.color = Color.black;
                label.alignment = TextAnchor.MiddleCenter;
                label.fontSize = Mathf.RoundToInt(buttonSize.y * 0.7f);
                label.transform.SetAsLastSibling();
            }
            else if (labelTMP != null)
            {
                labelTMP.text = MapDisplayName(name);
                labelTMP.gameObject.SetActive(true);
                labelTMP.color = Color.black;
                labelTMP.alignment = TextAlignmentOptions.Center;
                labelTMP.fontSize = Mathf.RoundToInt(buttonSize.y * 0.7f);
                labelTMP.rectTransform.anchorMin = Vector2.zero;
                labelTMP.rectTransform.anchorMax = Vector2.one;
                labelTMP.rectTransform.offsetMin = Vector2.zero;
                labelTMP.rectTransform.offsetMax = Vector2.zero;
                labelTMP.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                labelTMP.transform.SetAsLastSibling();
            }
            var layoutElement = b.gameObject.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layoutElement == null) layoutElement = b.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElement.preferredWidth = buttonSize.x;
            layoutElement.preferredHeight = buttonSize.y;
            layoutElement.flexibleHeight = 0f;
            layoutElement.flexibleWidth = 0f;
            // color block
            var cb = b.colors;
            cb.normalColor = buttonColor;
            cb.highlightedColor = buttonHoverColor;
            b.colors = cb;

            // tenta carregar miniatura via Resources; se não houver, deixa thumbnail invisível
            var thumbImage = b.transform.Find("Thumbnail")?.GetComponent<UnityEngine.UI.Image>();
            if (thumbImage != null && thumbImage.gameObject.name == "Thumbnail")
            {
                Sprite s = null;
                if (!string.IsNullOrEmpty(thumbnailResourcePath))
                    s = Resources.Load<Sprite>($"{thumbnailResourcePath}/{name}");
                if (s != null)
                {
                    thumbImage.sprite = s;
                    thumbImage.color = Color.white;
                    thumbImage.gameObject.SetActive(true);
                }
                else
                {
                    // não exibir thumbnail se não existir
                    thumbImage.gameObject.SetActive(false);
                }
            }

            string sceneName = name; // capture
            // aplica borda caso disponivel (Upgrade_bubble) — deixa o texto dentro da borda
            var bkg = b.GetComponent<UnityEngine.UI.Image>();
            // Ensure background is visible (fallback) — sometimes prefab uses full transparent image
            if (bkg != null)
            {
                if (forceVisibleButtons && bkg.color.a < 0.1f)
                {
                    bkg.color = Color.white; // fallback visible background
                }
            }
            if (buttonBorderSprite != null && bkg != null)
            {
                bkg.sprite = buttonBorderSprite;
                bkg.type = Image.Type.Sliced;
                // Ajusta tamanho do botão para caber o sprite, se desejar
                if (useBorderSpriteSize && layoutElement != null && buttonBorderSprite != null)
                {
                    // sprite.rect dá tamanho em pixels; UI rects também usam pixels
                    layoutElement.preferredWidth = buttonBorderSprite.rect.width;
                    layoutElement.preferredHeight = buttonBorderSprite.rect.height;
                }
                else if (bkg.sprite == null)
                {
                    // If the border sprite is missing, try a Resources fallback (helpful on builds)
                    var res = Resources.Load<Sprite>("Upgrade_bubble");
                    if (res != null)
                    {
                        bkg.sprite = res;
                        bkg.type = Image.Type.Sliced;
                        Debug.Log("MapSelectionUI: Applied Resources fallback border sprite for a button.");
                    }
                    else
                    {
                        // no sprite found: try to add a simple outline to make the button visible
                        Debug.LogWarning("MapSelectionUI: buttonBorderSprite missing and Resources fallback not found; adding Outline fallback.");
                        try
                        {
                            var outline = bkg.gameObject.GetComponent<UnityEngine.UI.Outline>();
                            if (outline == null) outline = bkg.gameObject.AddComponent<UnityEngine.UI.Outline>();
                            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
                            outline.effectDistance = new Vector2(4f, -4f);
                            // Keep a visible background
                            bkg.color = new Color(1f, 1f, 1f, 0.97f);
                        }
                        catch { }
                    }
                }
                // ensure button background is visible when using border sprite
                bkg.color = Color.white;

                // if panel background is dark, prefer dark text; else black text
                Color panelCol = Color.black;
                var panelImg = panel?.GetComponent<UnityEngine.UI.Image>();
                if (panelImg != null) panelCol = panelImg.color;
                bool panelIsDark = (panelCol.r + panelCol.g + panelCol.b) / 3f < 0.5f;
                if (label != null) label.color = panelIsDark ? Color.white : Color.black;
                if (labelTMP != null) labelTMP.color = panelIsDark ? Color.white : Color.black;

                // se a borda estiver presente, escondemos a thumbnail (não precisamos) e centralizamos o label
                if (thumbImage != null) thumbImage.gameObject.SetActive(false);
                // if we hide the thumbnail and use a border, disable the HorizontalLayoutGroup so label can fill the button
                var hl = b.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                if (hl != null) hl.enabled = false;

                if (label != null)
                {
                    label.alignment = TextAnchor.MiddleCenter;
                    // ensure label expands to fill the button so text appears inside the border
                    var le = label.GetComponent<UnityEngine.UI.LayoutElement>();
                    if (le == null) le = label.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                    le.flexibleWidth = 1f;
                    le.flexibleHeight = 1f;
                    if (labelTMP != null)
                    {
                        var leTMP = labelTMP.GetComponent<UnityEngine.UI.LayoutElement>();
                        if (leTMP == null) leTMP = labelTMP.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                        leTMP.flexibleWidth = 1f;
                        leTMP.flexibleHeight = 1f;
                    }
                    label.rectTransform.anchorMin = Vector2.zero;
                    label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.offsetMin = Vector2.zero;
                    label.rectTransform.offsetMax = Vector2.zero;
                    // ensure label stretches fully when HorizontalLayoutGroup is disabled
                    label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                }
            }
            b.onClick.AddListener(() => {
                Debug.Log($"MapSelectionUI: Botão {sceneName} clicado.");
                LoadMap(sceneName);
            });
            // Debug: show visual state
            Debug.Log($"MapSelectionUI: Created button {b.name} bgAlpha={ (b.GetComponent<UnityEngine.UI.Image>()?.color.a ?? -1f) } sprite={(b.GetComponent<UnityEngine.UI.Image>()?.sprite!=null)} label={(label!=null)} labelTMP={(labelTMP!=null)}");
        }

        // DEBUG: show how many scenes we found in Build Settings (or that we skipped because of simulateWebGL)
        Debug.Log($"MapSelectionUI: Found {scenes.Count} map scenes in Build Settings that start with '{scenePrefix}' (simulateWebGL={simulateWebGL})");

        // If nothing in Build Settings and we have a MapList ScriptableObject, use it as fallback
        if (scenes.Count == 0 && mapList == null)
        {
            // Try to load MapList from Resources
            mapList = Resources.Load<MapList>("MapList");
            if (mapList != null) Debug.Log($"MapSelectionUI: Loaded MapList ScriptableObject from Resources: {mapList.scenes.Count} scenes");
        }

        if (scenes.Count == 0 && mapList != null)
        {
            Debug.Log($"MapSelectionUI: Populate from MapList with {mapList.scenes.Count} scenes");
            PopulateFromMapList(mapList);
            Debug.Log($"MapSelectionUI: Buttons created after PopulateFromMapList: {contentParent.childCount}");
            // scenes is now set by PopulateFromMapList
            return;
        }

        // Reorder scenes to prefer default Map order if present
        var preferred = new List<string> { "Map_first", "Map_div", "Map_split" };
        scenes.Sort((a,b) =>
        {
            int ai = preferred.IndexOf(a);
            int bi = preferred.IndexOf(b);
            if (ai == -1 && bi == -1) return a.CompareTo(b);
            if (ai == -1) return 1;
            if (bi == -1) return -1;
            return ai.CompareTo(bi);
        });

        // If older repo layout is not included in BuildSettings, prefer the MapList ScriptableObject (WebGL)
        // The Assets/Map/Scenes fallback was removed to simplify the editor/runtime logic.

        // se quiser forçar layout horizontal no contentParent (por exemplo se usar fallback)
        if (useHorizontalLayout && contentParent != null)
        {
            var v = contentParent.GetComponent<VerticalLayoutGroup>();
            if (v != null) Destroy(v);
            if (contentParent.GetComponent<HorizontalLayoutGroup>() == null)
            {
                var h = contentParent.gameObject.AddComponent<HorizontalLayoutGroup>();
                h.spacing = 10f;
                h.childControlWidth = true;
                h.childControlHeight = false;
                h.childForceExpandHeight = false;
                h.childForceExpandWidth = false;
            }
        }

        // If no map scenes were found, inform user in the content area so WebGL builds show feedback
        if (scenes.Count == 0 && contentParent != null)
        {
            Debug.LogWarning("MapSelectionUI: Nenhum mapa encontrado nas Build Settings. Verifique se cenas de mapas estão incluídas na build.");
            var msgGo = new GameObject("NoMapsMessage");
            msgGo.transform.SetParent(contentParent, false);
            var txt = msgGo.AddComponent<UnityEngine.UI.Text>();
            txt.text = "Nenhum mapa disponível. Verifique Build Settings (WebGL não lista arquivos locais).";
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 24;
            Font f = null; try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { } if (f == null) { try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } } if (f != null) txt.font = f;
            var rtmsg = msgGo.GetComponent<RectTransform>(); rtmsg.sizeDelta = new Vector2(600, 100);
            Debug.Log("MapSelectionUI: No scenes found in Build Settings and no MapList found; created NoMapsMessage.");
        }
    }

    // Populate UI using a ScriptableObject MapList - this works in builds where BuildSettings scene list is empty (WebGL)
    private void PopulateFromMapList(MapList ml)
    {
        if (ml == null || ml.scenes == null) return;
        foreach (var name in ml.scenes)
        {
            if (string.IsNullOrEmpty(name)) continue;
            if (!string.IsNullOrEmpty(scenePrefix) && !name.StartsWith(scenePrefix)) continue;
            if (!scenes.Contains(name)) scenes.Add(name);

            var b = Instantiate(buttonPrefab, contentParent);
            b.name = "MapButton_" + name;
            var label = b.GetComponentInChildren<Text>();
            var labelTMP = b.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) { label.text = MapDisplayName(name); label.gameObject.SetActive(true); label.color = Color.black; label.transform.SetAsLastSibling(); label.fontSize = Mathf.RoundToInt(buttonSize.y * 0.7f); }
            if (labelTMP != null) { labelTMP.text = MapDisplayName(name); labelTMP.gameObject.SetActive(true); labelTMP.color = Color.black; labelTMP.transform.SetAsLastSibling(); labelTMP.fontSize = Mathf.RoundToInt(buttonSize.y * 0.7f); }

            // If panel is dark, invert text color for readability
            var panelImg = panel?.GetComponent<UnityEngine.UI.Image>();
            if (panelImg != null)
            {
                bool panelIsDark = (panelImg.color.r + panelImg.color.g + panelImg.color.b) / 3f < 0.5f;
                if (label != null) label.color = panelIsDark ? Color.white : Color.black;
                if (labelTMP != null) labelTMP.color = panelIsDark ? Color.white : Color.black;
            }

            // basic resize and event
            var rt = b.GetComponent<RectTransform>(); if (rt != null) rt.sizeDelta = buttonSize;
            var layoutElement = b.gameObject.GetComponent<UnityEngine.UI.LayoutElement>(); if (layoutElement == null) layoutElement = b.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElement.preferredWidth = buttonSize.x; layoutElement.preferredHeight = buttonSize.y; layoutElement.flexibleHeight = 0f; layoutElement.flexibleWidth = 0f;
            string ncopy = name;
            b.onClick.AddListener(() => { Debug.Log($"MapSelectionUI: MapList button {ncopy} clicked."); LoadMap(ncopy); });
                Debug.Log($"MapSelectionUI: Created MapList button {b.name} bgAlpha={(b.GetComponent<UnityEngine.UI.Image>()?.color.a ?? -1f)} sprite={(b.GetComponent<UnityEngine.UI.Image>()?.sprite!=null)}");

            // Apply default visual style to MapList buttons (similar to PopulateList)
            var bkg = b.GetComponent<UnityEngine.UI.Image>();
            if (bkg != null && forceVisibleButtons && bkg.color.a < 0.1f)
            {
                bkg.color = Color.white;
            }
            if (bkg != null)
            {
                // Make sure a visible background exists (fallback)
                bkg.color = Color.white;
                if (forceVisibleButtons && bkg.color.a < 0.1f) bkg.color = Color.white;
                if (buttonBorderSprite != null)
                {
                    bkg.sprite = buttonBorderSprite;
                    bkg.type = Image.Type.Sliced;
                }
                else
                {
                    // Try to load a fallback sprite from Resources when using MapList fallback
                    var res = Resources.Load<Sprite>("Upgrade_bubble");
                    if (res != null)
                    {
                        bkg.sprite = res;
                        bkg.type = Image.Type.Sliced;
                        Debug.Log("MapSelectionUI: Applied Resources fallback border sprite for MapList button.");
                    }
                    else
                    {
                        // Add an outline/shadow fallback if sprite not present, to make the button stand out
                        Debug.LogWarning("MapSelectionUI: buttonBorderSprite missing for MapList and no Resources fallback; adding Outline fallback.");
                        try
                        {
                            var outline = bkg.gameObject.GetComponent<UnityEngine.UI.Outline>();
                            if (outline == null) outline = bkg.gameObject.AddComponent<UnityEngine.UI.Outline>();
                            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
                            outline.effectDistance = new Vector2(3f, -3f);
                        }
                        catch { }
                    }
                }
            }
            // Colors
            try {
                var cb = b.colors;
                cb.normalColor = buttonColor;
                cb.highlightedColor = buttonHoverColor;
                b.colors = cb;
            } catch {}

            // Thumbnail: try to hide it if missing (same as PopulateList)
            var thumbImage = b.transform.Find("Thumbnail")?.GetComponent<UnityEngine.UI.Image>();
            if (thumbImage != null)
            {
                Sprite s = null;
                if (!string.IsNullOrEmpty(thumbnailResourcePath))
                    s = Resources.Load<Sprite>($"{thumbnailResourcePath}/{name}");
                if (s != null)
                {
                    thumbImage.sprite = s;
                    thumbImage.color = Color.white;
                    thumbImage.gameObject.SetActive(true);
                }
                else
                {
                    thumbImage.gameObject.SetActive(false);
                }
            }
            }
            Debug.Log($"MapSelectionUI: CreateFromMapList created {contentParent.childCount} buttons");
        }

    public void LoadMap(string sceneName)
    {
        // opcional: aqui podemos salvar a preferencia de mapa antes de iniciar
        Debug.Log($"MapSelectionUI: Carregando cena {sceneName}");

        // se temos o caminho do arquivo, podemos checar se a cena está nas Build Settings
        if (scenePaths.TryGetValue(sceneName, out var path))
        {
            int buildIndex = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(path);
            if (buildIndex == -1)
            {
#if UNITY_EDITOR
                // Em Editor: perguntar para adicionar a cena ao Build Settings e carregar
                bool add = UnityEditor.EditorUtility.DisplayDialog("Adicionar cena ao Build Settings?",
                    $"A cena '{sceneName}' não está nas Build Settings. Deseja adicioná-la e carregar?", "Sim", "Não");
                if (add)
                {
                    var list = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>(UnityEditor.EditorBuildSettings.scenes);
                    list.Add(new UnityEditor.EditorBuildSettingsScene(path, true));
                    UnityEditor.EditorBuildSettings.scenes = list.ToArray();
                    Debug.Log($"MapSelectionUI: Adicionada cena '{sceneName}' em Build Settings.");
                }
                else
                {
                    Debug.LogWarning($"MapSelectionUI: Cena '{sceneName}' não foi adicionada. Cancelado.");
                }
#else
                Debug.LogError($"Scene '{sceneName}' couldn't be loaded because it's not in the Build Settings.");
                return;
#endif
            }
        }

        // Usa o nome (ou path) para carregar a cena — se estiver nas Build Settings, carregar por nome funciona.
        SceneManager.LoadScene(sceneName);
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
        // atualizar lista sempre que abrir
        PopulateList();
        // if nothing created, try fallback runtime builder (resources or temporary)
        if (contentParent != null && contentParent.childCount == 0)
        {
            Debug.LogWarning("MapSelectionUI: No map buttons created — trying runtime fallback (MapList/temporary). If you used Build Profiles, ensure maps are added for WebGL.");
            if (mapList != null && mapList.scenes.Count > 0)
            {
                PopulateFromMapList(mapList);
                return;
            }

            // final fallback: create a temporary MapSelection panel
            if (panel == null || !panel.name.Equals("MapSelectionPanel"))
            {
                var tempPanel = CreateTemporaryAndShow();
                if (tempPanel != null)
                {
                    Debug.Log("MapSelectionUI: Using temporary runtime fallback in Show().");
                }
            }
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    /// <summary>
    /// Cria um painel básico de seleção de mapas em runtime (fallback) e já mostra.
    /// Útil para o Menu principal quando o designer não montou a UI na cena.
    /// </summary>
    public static MapSelectionUI CreateTemporaryAndShow()
    {
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            // cria um Canvas básico
            var cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // ensure EventSystem exists so buttons work in standalone builds
        if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // painel
        var panelGo = new GameObject("MapSelectionPanel");
        panelGo.transform.SetParent(canvas.transform, false);
        var img = panelGo.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0f, 0f, 0f, 0.7f);
        var rect = panelGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var content = new GameObject("Content");
        content.transform.SetParent(panelGo.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.25f, 0.2f);
        contentRect.anchorMax = new Vector2(0.75f, 0.8f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        var layout = content.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        // Centraliza os filhos (verticalmente) dentro do Content
        layout.childAlignment = TextAnchor.MiddleCenter;

        var csf = content.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        csf.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        // cria prefab simples de botao
        var btnPrefab = new GameObject("MapButtonPrefab");
        var btnRect = btnPrefab.AddComponent<RectTransform>();
        btnPrefab.AddComponent<CanvasRenderer>();
        var btnImage = btnPrefab.AddComponent<UnityEngine.UI.Image>();
        // start visible by default so buttons are not invisible in fallback mode
        btnImage.color = new Color(1f, 1f, 1f, 0.95f);
        var btn = btnPrefab.AddComponent<UnityEngine.UI.Button>();
        // layout para thumb + label
        var hLayout = btnPrefab.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = false;
        hLayout.childForceExpandHeight = false;
        hLayout.spacing = 8f;

        var thumb = new GameObject("Thumbnail");
        thumb.transform.SetParent(btnPrefab.transform, false);
        var thumbImg = thumb.AddComponent<UnityEngine.UI.Image>();
        thumbImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        var thumbLE = thumb.AddComponent<UnityEngine.UI.LayoutElement>();
        thumbLE.preferredWidth = 80f;
        thumbLE.preferredHeight = 60f;

        var txtGo = new GameObject("Text");
        txtGo.transform.SetParent(btnPrefab.transform, false);
        var txt = txtGo.AddComponent<UnityEngine.UI.Text>();
        txt.color = Color.black; // user requested black text
        txt.alignment = TextAnchor.MiddleCenter;
        // Unity versions 2024+ deprecate Arial.ttf as builtin. Try LegacyRuntime.ttf first, fallback to Arial if available.
        Font builtinFont = null;
        try
        {
            builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch { builtinFont = null; }

        if (builtinFont == null)
        {
            try { builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { builtinFont = null; }
        }

        if (builtinFont != null) txt.font = builtinFont;
        txt.text = "Map";
        var txtLE = txtGo.AddComponent<UnityEngine.UI.LayoutElement>();
        txtLE.flexibleWidth = 1f;
        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = Vector2.zero;
        txt.rectTransform.offsetMax = Vector2.zero;

        // adiciona o MapSelectionUI
        var ms = panelGo.AddComponent<MapSelectionUI>();
        ms.panel = panelGo;
        ms.contentParent = content.transform;
        ms.buttonPrefab = btn;

        // Assign upgrade bubble border immediately so Show() can use it
        #if UNITY_EDITOR
        if (ms.buttonBorderSprite == null)
        {
            var defaultPath = "Assets/Ian/Gameplay Hud/Upgrade_bubble.png";
            var sp2 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(defaultPath);
            if (sp2 != null) ms.buttonBorderSprite = sp2;
        }
        #endif

        // Runtime fallback for border sprite (if not assigned). Put the sprite at Assets/Resources/Upgrade_bubble.png to be found.
        if (ms.buttonBorderSprite == null)
        {
            var sp = Resources.Load<Sprite>("Upgrade_bubble");
            if (sp != null) ms.buttonBorderSprite = sp;
        }

        // não exibe imediatamente para evitar conflito visual; deixe o Show() cuidar disso
        if (ms.panel != null) ms.panel.SetActive(false);

        // mostra imediatamente
        ms.Show();
        Debug.Log("MapSelectionUI: fallback criado em runtime and shown; default btn alpha=" + btnImage.color.a);
        return ms;
    }
}
