using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    
    private Vector2 movement;
    private Vector2 lastMovement;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Captura input do jogador (suporta teclado e mobile)
        movement = GetMovementInput();
        
        // Atualiza animações
        if (animator != null)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);
            
            // Salva a última direção de movimento para idle
            if (movement != Vector2.zero)
            {
                lastMovement = movement;
                animator.SetFloat("LastHorizontal", lastMovement.x);
                animator.SetFloat("LastVertical", lastMovement.y);
            }
        }
    }
    
    void FixedUpdate()
    {
        // Move o personagem
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
    
    /// <summary>
    /// Obtém input de movimento (teclado ou joystick virtual)
    /// </summary>
    private Vector2 GetMovementInput()
    {
        Vector2 input = Vector2.zero;
        
        // Tenta pegar input do joystick virtual primeiro (mobile)
        if (VirtualJoystick.Instance != null)
        {
            input = VirtualJoystick.Instance.GetInput();
        }
        
        // Se não tiver input do joystick, usa teclado/gamepad
        if (input.sqrMagnitude < 0.01f)
        {
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");
        }
        
        return input;
    }
}
