using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{

    public void IniciarJogo()
    {
        GameController.Init();
        // ao invés de iniciar jogo diretamente, abre a tela de seleção de mapas se estiver disponível
        var mapSelection = UnityEngine.Object.FindFirstObjectByType<MapSelectionUI>();
        if (mapSelection != null && mapSelection.panel != null)
        {
            mapSelection.Show();
            return;
        }

        // se o MapSelectionUI não estiver configurado na cena, cria um fallback em runtime
        mapSelection = MapSelectionUI.CreateTemporaryAndShow();
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