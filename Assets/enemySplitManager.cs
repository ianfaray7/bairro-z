using System;
using UnityEngine;

public class enemySplitManager : MonoBehaviour
{
    public Transform[] checkpoints;

    public static enemySplitManager main;

    private void Awake()
    {
        main = this;
    }
}