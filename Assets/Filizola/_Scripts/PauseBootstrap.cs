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
            }
        }
        catch { }
    }
}