using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    // Ir para a fase / cena principal de jogo
    public void IniciarJogo()
    {
        Time.timeScale = 1f;                       // garante jogo “despausado”
        SceneManager.LoadScene("SampleScene");     // nome exato da sua cena de jogo
    }

    // Voltar para o menu principal
    public void IrParaMenuPrincipal()
    {
        Time.timeScale = 1f;                       // tira o pause
        SceneManager.LoadScene("MainMenu");        // nome exato da sua cena de menu
    }

    // Sair do jogo (se quiser usar em algum botão)
    public void SairJogo()
    {
        Debug.Log("Saindo do jogo…");
        Application.Quit();
    }
}
