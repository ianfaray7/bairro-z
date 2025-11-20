using UnityEngine;

public class GameController : MonoBehaviour
{
    // Pontuação do jogador
    public static int score = 0;

    // Usado pelo UIManager para saber se o jogo acabou
    public static bool gameOver = false;

    // Referência à UI de pause
    public GameObject pauseMenuUI;

    void Start()
    {
        // Garante que o jogo comece inicializado
        Init();

        // Garante que o pause comece desativado
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    // ==== MÉTODO QUE O MENU USA PARA INICIAR / REINICIAR O JOGO ====
    public static void Init()
    {
        score = 0;
        gameOver = false;

        // Garante que o jogo esteja rodando na velocidade normal
        Time.timeScale = 1f;

        Debug.Log("GameController.Init() chamado: jogo inicializado.");
    }

    // Chamado quando um item é coletado
    public static void Collect()
    {
        score++;
        Debug.Log("Collect chamado no GameController. Pontuação: " + score);
    }

    // PAUSAR O JOGO
    public void PauseGame()
    {
        Time.timeScale = 0f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    // RETOMAR O JOGO
    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    // SAIR DO JOGO
    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }

    // OPCIONAL: se algum dia quiser marcar game over
    public static void SetGameOver()
    {
        gameOver = true;
        Debug.Log("Game Over!");
    }
}
