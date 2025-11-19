using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FlipByMouse : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCam;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;
    }

    void Update()
    {
        if (mainCam == null) return;

        // Pega a posição do mouse na tela e converte para coordenadas do mundo
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(mouseScreen);

        // Se o mouse estiver à esquerda do player, vira o sprite
        if (mouseWorld.x < transform.position.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
    }
}
