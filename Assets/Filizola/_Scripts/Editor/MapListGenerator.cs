#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MapListGenerator
{
    [MenuItem("Tools/Filizola/Generate MapList from Build Settings")]
    public static void GenerateMapList()
    {
        var asset = ScriptableObject.CreateInstance<MapList>();
        var scenes = new List<string>();
        foreach (var s in UnityEditor.EditorBuildSettings.scenes)
        {
            var path = s.path;
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(name) && name.StartsWith("Map_"))
            {
                scenes.Add(name);
            }
        }

        asset.scenes = scenes;
        string dest = "Assets/Resources/MapList.asset";
        // ensure directory exists
        var dir = System.IO.Path.GetDirectoryName(dest);
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

        AssetDatabase.CreateAsset(asset, dest);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"MapList generated at {dest} with {scenes.Count} scenes.");

        // Ensure the border sprite is included in Resources for WebGL builds
        var defaultSpritePath = "Assets/Ian/Gameplay Hud/Upgrade_bubble.png";
        var resourceSpriteDest = "Assets/Resources/Upgrade_bubble.png";
        // Always update the resource sprite to match the source if available
        if (System.IO.File.Exists(defaultSpritePath))
        {
            // Remove existing in Resources if present, then copy
            if (AssetDatabase.LoadAssetAtPath<Sprite>(resourceSpriteDest) != null)
            {
                AssetDatabase.DeleteAsset(resourceSpriteDest);
            }
            var copied = AssetDatabase.CopyAsset(defaultSpritePath, resourceSpriteDest);
            if (copied)
            {
                AssetDatabase.ImportAsset(resourceSpriteDest);
                Debug.Log($"MapListGenerator: Copied '{defaultSpritePath}' to '{resourceSpriteDest}' to ensure border sprite is in build.");
            }
            else
            {
                Debug.LogWarning($"MapListGenerator: Could not copy sprite from '{defaultSpritePath}' to Resources.");
            }
        }
        else
        {
            // Try searching by name for any sprite named 'Upgrade_bubble'
            var guids = AssetDatabase.FindAssets("Upgrade_bubble t:Sprite");
            if (guids.Length > 0)
            {
                var foundPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                Debug.Log($"MapListGenerator: found sprite by name at {foundPath}; copying to Resources.");
                if (AssetDatabase.LoadAssetAtPath<Sprite>(resourceSpriteDest) != null)
                    AssetDatabase.DeleteAsset(resourceSpriteDest);
                var ok = AssetDatabase.CopyAsset(foundPath, resourceSpriteDest);
                if (ok) AssetDatabase.ImportAsset(resourceSpriteDest);
                else Debug.LogWarning($"MapListGenerator: could not copy found sprite '{foundPath}' to Resources");
            }
            else
            {
                Debug.LogWarning($"MapListGenerator: default sprite not found at '{defaultSpritePath}'; WebGL may not show button borders.");
            }
    }
    }
}
#endif