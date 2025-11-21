// This runs before building and regenerates the MapList ScriptableObject from Build Settings.
#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class MapListPrebuild : IPreprocessBuildWithReport
{
    // run early
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        try
        {
            // Generate MapList from active Build Settings so WebGL builds always have a MapList present
            MapListGenerator.GenerateMapList();
            Debug.Log("MapListPrebuild: Generated MapList asset before build.");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("MapListPrebuild: Could not generate MapList before build: " + ex.Message);
        }
    }
}
#endif