using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapList", menuName = "Filizola/Map List", order = 1)]
public class MapList : ScriptableObject
{
    public List<string> scenes = new List<string>();
}