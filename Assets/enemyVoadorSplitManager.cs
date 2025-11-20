using System;
using UnityEngine;

public class enemyVoadorSplitManager : MonoBehaviour
{
    public Transform[] checkpoints;

    public static enemyVoadorSplitManager main;

    private void Awake()
    {
        main = this;
    }
}