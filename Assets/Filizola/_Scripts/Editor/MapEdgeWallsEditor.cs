using UnityEditor;
using UnityEngine;

public class MapEdgeWallsEditor
{
    [MenuItem("Filizola/Map Edge/Ensure Player Layers")] 
    public static void EnsurePlayerLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layersProp = tagManager.FindProperty("layers");
        bool changed = false;
        string[] wanted = new string[] { "Player", "PlayerWall" };
        for (int i = 8; i < layersProp.arraySize && i < 32; i++)
        {
            var element = layersProp.GetArrayElementAtIndex(i);
            string current = element.stringValue;
            for (int j = 0; j < wanted.Length; j++)
            {
                if (current == wanted[j])
                {
                    wanted[j] = null; // already exists
                }
            }
        }

        // fill remaining layers with wanted
        for (int i = 8; i < layersProp.arraySize && i < 32; i++)
        {
            if (layersProp.GetArrayElementAtIndex(i).stringValue == "")
            {
                // find wanted item
                for (int j = 0; j < wanted.Length; j++)
                {
                    if (wanted[j] != null)
                    {
                        layersProp.GetArrayElementAtIndex(i).stringValue = wanted[j];
                        wanted[j] = null;
                        changed = true;
                        break;
                    }
                }
            }
        }
        if (changed)
        {
            tagManager.ApplyModifiedProperties();
            Debug.Log("MapEdgeWallsEditor: Adicionadas as layers Player/PlayerWall nas Tags & Layers (Project Settings).\nRevise se deseja personalizar o nome das layers.");
        }
        else
        {
            Debug.Log("MapEdgeWallsEditor: As layers Player/PlayerWall já existem ou não há espaço livre a partir do slot 8.");
        }
    }
}
