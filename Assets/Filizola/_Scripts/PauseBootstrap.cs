using UnityEngine;
using UnityEngine.SceneManagement;

public static class PauseBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        try
        {
            if (scene.name != null && scene.name.StartsWith("Map_"))
            {
                if (UnityEngine.Object.FindFirstObjectByType<PauseMenu>() == null)
                {
                    var go = new GameObject("PauseMenu");
                    go.AddComponent<PauseMenu>();
                }
                // Ensure UIManager exists so game over UI is present
                if (UnityEngine.Object.FindFirstObjectByType<UIManager>() == null)
                {
                    var uiGo = new GameObject("UIManager");
                    uiGo.AddComponent<UIManager>();
                }
            }
        }
        catch { }
    }
}