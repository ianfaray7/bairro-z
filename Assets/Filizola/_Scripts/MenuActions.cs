using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{

    public void IniciarJogo()
    {
        GameController.Init();
        // ensure we are not paused when starting the game menu or switching scenes
        PauseManager.Resume();
        // ao invés de iniciar jogo diretamente, abre a tela de seleção de mapas se estiver disponível
        var mapSelection = UnityEngine.Object.FindFirstObjectByType<MapSelectionUI>();
        Debug.Log($"MenuActions: IniciarJogo clicked. mapSelection present: {mapSelection != null}");
        if (mapSelection != null)
        {
            // If configured in scene, show it; otherwise create a robust fallback
            if (mapSelection.panel != null && mapSelection.contentParent != null && mapSelection.buttonPrefab != null)
            {
                mapSelection.Show();
                return;
            }
            Debug.LogWarning("MenuActions: MapSelectionUI exists but is not fully configured - creating runtime fallback.");
        }

        // se o MapSelectionUI não estiver configurado na cena, cria um fallback em runtime
        mapSelection = MapSelectionUI.CreateTemporaryAndShow();
        Debug.Log($"MenuActions: Created temporary MapSelectionUI: {mapSelection != null}");
        if (mapSelection != null) return;

        // fallback final: carrega a cena padrão (índice 1)
        SceneManager.LoadScene(1);
    }

    public void Menu()
    {
        SceneManager.LoadScene(0);
    }

    void Start()
    {
        // Corrige cor do texto "Iniciar" no menu para branco se encontrado na cena atual.
        var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            var texts = canvas.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var t in texts)
            {
                if (string.IsNullOrEmpty(t.text)) continue;
                var low = t.text.ToLower();
                if (low.Contains("iniciar") || low.Contains("jogo") || low.Contains("play"))
                {
                    t.color = Color.white;
                }
            }
        }
    }
}