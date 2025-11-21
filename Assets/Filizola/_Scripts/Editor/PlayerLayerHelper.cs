using UnityEditor;
using UnityEngine;

public class PlayerLayerHelper
{
    [MenuItem("Filizola/Map Edge/Set Selected GameObject as Player Layer")] 
    public static void SetSelectedToPlayerLayer()
    {
        var go = Selection.activeGameObject;
        if (go == null) { Debug.LogWarning("Selecione um GameObject para definir a layer Player."); return; }
        int layer = LayerMask.NameToLayer("Player");
        if (layer < 0) { Debug.LogWarning("Layer Player não existe. Use Filizola/Map Edge/Ensure Player Layers para criar."); return; }
        go.layer = layer;
        Debug.Log($"GameObject {go.name} definido para layer 'Player'.");
    }
}
