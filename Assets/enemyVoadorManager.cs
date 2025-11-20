using System;
using UnityEngine;

public class enemyVoadorManager : MonoBehaviour
{
    public Transform[] checkpoints;

    public static enemyVoadorManager main;

    private void Awake()
    {
        main = this;
    }
}
