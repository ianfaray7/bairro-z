using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PauseableRigidbody : MonoBehaviour
{
    Rigidbody2D rb;
    bool wasSimulated;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        PauseManager.OnPauseChanged += OnPauseChanged;
    }

    void OnDisable()
    {
        PauseManager.OnPauseChanged -= OnPauseChanged;
    }

    private void Start()
    {
        if (rb != null) wasSimulated = rb.simulated;
    }

    void OnPauseChanged(bool paused)
    {
        if (rb == null) return;
        if (paused)
        {
            wasSimulated = rb.simulated;
            rb.simulated = false;
            // stop moving
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            rb.simulated = wasSimulated;
        }
    }
}