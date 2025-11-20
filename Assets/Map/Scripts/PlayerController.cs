using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    Rigidbody2D rb;
    Vector2 moveInput;

    // info público para que armas usem
    public Vector2 MouseWorldPosition { get; private set; }
    public float AimAngle { get; private set; } // em graus, 0 = direita

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // input de movimento (WASD / setas)
        float hx = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        float vy = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(hx, vy).normalized;

        // mouse -> mundo
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld3 = Camera.main.ScreenToWorldPoint(mouseScreen);
        MouseWorldPosition = new Vector2(mouseWorld3.x, mouseWorld3.y);

        // calc angle (em graus)
        Vector2 dir = MouseWorldPosition - (Vector2)transform.position;
        AimAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
