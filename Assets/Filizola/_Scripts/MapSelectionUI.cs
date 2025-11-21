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
    [Header("Layout Options")]
    public bool useHorizontalLayout = true;
    public Sprite buttonBorderSprite; // arraste aqui o 'Artboard 5' do Inspector
    public bool useBorderSpriteSize = true; // se true, o botão terá o tamanho do sprite de borda

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

        int count = SceneManager.sceneCountInBuildSettings;
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
            var label = b.GetComponentInChildren<Text>();
            var labelTMP = b.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = MapDisplayName(name);
                label.gameObject.SetActive(true);
                // escrevemos o texto em preto conforme solicitado
                label.color = Color.black;
                label.fontSize = Mathf.RoundToInt(buttonSize.y * 0.35f);
                label.alignment = TextAnchor.MiddleCenter;
                    label.transform.SetAsLastSibling();
            }
            else if (labelTMP != null)
            {
                labelTMP.text = MapDisplayName(name);
                labelTMP.gameObject.SetActive(true);
                labelTMP.color = Color.black;
                labelTMP.fontSize = Mathf.RoundToInt(buttonSize.y * 0.35f);
                labelTMP.alignment = TextAlignmentOptions.Center;
                labelTMP.rectTransform.anchorMin = Vector2.zero;
                labelTMP.rectTransform.anchorMax = Vector2.one;
                labelTMP.rectTransform.offsetMin = Vector2.zero;
                labelTMP.rectTransform.offsetMax = Vector2.zero;
                labelTMP.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    labelTMP.transform.SetAsLastSibling();
            }

            // Ajusta aparência do botão
            var rt = b.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = buttonSize;
            // Ensure LayoutElement forces a preferred size and prevents vertical stretching
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
                // ensure button background is visible when using border sprite
                bkg.color = Color.white;

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

        // caso poucas cenas encontradas no BuildSettings, procurar nas pastas do projeto (Assets/Map/Scenes)
        if (scenes.Count < 3)
        {
            try
            {
                var dir = Path.Combine(Application.dataPath, "Map/Scenes");
                if (Directory.Exists(dir))
                {
                    var files = Directory.GetFiles(dir, "*.unity");
                    foreach (var f in files)
                    {
                        var n = Path.GetFileNameWithoutExtension(f);
                        if (string.IsNullOrEmpty(n)) continue;
                        if (!string.IsNullOrEmpty(scenePrefix) && !n.StartsWith(scenePrefix)) continue;
                        if (!scenes.Contains(n))
                        {
                            // adiciona botão
                            scenes.Add(n);
                            // capture path for fallback entries too
                            if (!scenePaths.ContainsKey(n)) scenePaths[n] = f.Replace("\\", "/");
                            var b2 = Instantiate(buttonPrefab, contentParent);
                            b2.name = "MapButton_" + n;
                            var label2 = b2.GetComponentInChildren<Text>();
                            var label2TMP = b2.GetComponentInChildren<TextMeshProUGUI>();
                            if (label2 != null)
                            {
                                label2.text = MapDisplayName(n);
                                label2.gameObject.SetActive(true);
                                // escrevemos o texto em preto conforme solicitado
                                label2.color = Color.black;
                                label2.alignment = TextAnchor.MiddleCenter;
                                    label2.transform.SetAsLastSibling();
                            }
                            else if (label2TMP != null)
                            {
                                label2TMP.text = MapDisplayName(n);
                                label2TMP.gameObject.SetActive(true);
                                label2TMP.color = Color.black;
                                label2TMP.alignment = TextAlignmentOptions.Center;
                                label2TMP.rectTransform.anchorMin = Vector2.zero;
                                label2TMP.rectTransform.anchorMax = Vector2.one;
                                label2TMP.rectTransform.offsetMin = Vector2.zero;
                                label2TMP.rectTransform.offsetMax = Vector2.zero;
                                label2TMP.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                                    label2TMP.transform.SetAsLastSibling();
                            }
                            var rt2 = b2.GetComponent<RectTransform>(); if (rt2 != null) rt2.sizeDelta = buttonSize;
                            var le2 = b2.gameObject.GetComponent<UnityEngine.UI.LayoutElement>();
                            if (le2 == null) le2 = b2.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                            le2.preferredWidth = buttonSize.x;
                            le2.preferredHeight = buttonSize.y;
                            le2.flexibleHeight = 0f;
                            le2.flexibleWidth = 0f;
                            var bkg2 = b2.GetComponent<UnityEngine.UI.Image>();
                            if (buttonBorderSprite != null && bkg2 != null) { bkg2.sprite = buttonBorderSprite; bkg2.type = Image.Type.Sliced; }
                                if (useBorderSpriteSize && buttonBorderSprite != null)
                                {
                                    le2.preferredWidth = buttonBorderSprite.rect.width;
                                    le2.preferredHeight = buttonBorderSprite.rect.height;
                                    // atualiza fontSize depois de ajustar o tamanho final do botão para fallback
                                    if (label2 != null)
                                    {
                                        label2.fontSize = Mathf.RoundToInt(le2.preferredHeight * 0.35f);
                                    }
                                }
                            var cb2 = b2.colors;
                            cb2.normalColor = buttonColor;
                            cb2.highlightedColor = buttonHoverColor;
                            b2.colors = cb2;
                            var thumbImage2 = b2.transform.Find("Thumbnail")?.GetComponent<UnityEngine.UI.Image>();
                            if (thumbImage2 != null)
                            {
                                Sprite s2 = null;
                                if (!string.IsNullOrEmpty(thumbnailResourcePath)) s2 = Resources.Load<Sprite>($"{thumbnailResourcePath}/{n}");
                                if (s2 != null)
                                {
                                    thumbImage2.sprite = s2;
                                    thumbImage2.color = Color.white;
                                    thumbImage2.gameObject.SetActive(true);
                                }
                                else
                                {
                                    // não exibir thumbnail se não existir - evita retângulos brancos
                                    thumbImage2.gameObject.SetActive(false);
                                    // também desativa a HorizontalLayoutGroup para centralizar label2
                                    var hl2 = b2.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                                    if (hl2 != null) hl2.enabled = false;
                                }
                            }
                            string ncopy = n;
                            b2.onClick.AddListener(() => {
                                Debug.Log($"MapSelectionUI: Botão {ncopy} clicado (fallback).");
                                LoadMap(ncopy);
                            });
                            // se a borda estiver presente, escondemos a thumbnail (não precisamos) e centralizamos o label
                            if (buttonBorderSprite != null)
                            {
                                if (thumbImage2 != null) thumbImage2.gameObject.SetActive(false);
                                if (label2 != null)
                                {
                                    label2.alignment = TextAnchor.MiddleCenter;
                                    var le = label2.GetComponent<UnityEngine.UI.LayoutElement>();
                                    if (le == null) le = label2.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                                    le.flexibleWidth = 1f;
                                    le.flexibleHeight = 1f;
                                    if (label2TMP != null)
                                    {
                                        var leTMP2 = label2TMP.GetComponent<UnityEngine.UI.LayoutElement>();
                                        if (leTMP2 == null) leTMP2 = label2TMP.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                                        leTMP2.flexibleWidth = 1f;
                                        leTMP2.flexibleHeight = 1f;
                                    }
                                    label2.rectTransform.anchorMin = Vector2.zero;
                                    label2.rectTransform.anchorMax = Vector2.one;
                                    label2.rectTransform.offsetMin = Vector2.zero;
                                    label2.rectTransform.offsetMax = Vector2.zero;
                                    // disable horizontal layout (fallback buttons) so label occupies center space
                                    var hl2 = b2.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                                    if (hl2 != null) hl2.enabled = false;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

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
        // start transparent: avoid visible white rectangles until border sprite is applied
        btnImage.color = new Color(1f, 1f, 1f, 0f);
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

        // não exibe imediatamente para evitar conflito visual; deixe o Show() cuidar disso
        if (ms.panel != null) ms.panel.SetActive(false);

        // mostra imediatamente
        ms.Show();
        Debug.Log("MapSelectionUI: fallback criado em runtime.");
        return ms;
    }
}
