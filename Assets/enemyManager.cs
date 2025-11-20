using System;
using UnityEngine;

public class enemyManager : MonoBehaviour
{
    public Transform[] checkpoints;

    public static enemyManager main;

    private void Awake()
    {
        main = this;
    }
}
