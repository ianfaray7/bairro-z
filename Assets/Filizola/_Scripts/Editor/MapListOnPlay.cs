#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Generate MapList before entering Play Mode so Editor Play emulates WebGL MapList fallback.
[InitializeOnLoad]
public static class MapListOnPlay
{
    static MapListOnPlay()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            try
            {
                // ensure MapList is regenerated from BuildSettings so WebGL fallback will be available in Play Mode
                MapListGenerator.GenerateMapList();
                Debug.Log("MapListOnPlay: Generated MapList before entering Play Mode.");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("MapListOnPlay: Could not generate MapList for Play Mode: " + ex.Message);
            }
        }
    }
}
#endif