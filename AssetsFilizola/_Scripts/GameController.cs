using UnityEngine;

public static class GameController
{
    private static int collectibleCount = 4;

    public static bool gameOver
    {
        get { return collectibleCount <= 0; }
    }

    

    public static void Init()
    {
        collectibleCount = 4;
    }
    
    public static void Collect()
    {
        collectibleCount--;
    }
}
