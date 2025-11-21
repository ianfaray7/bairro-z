using UnityEngine;

public class PlayerMovementFilizola : MonoBehaviour
{

    private Rigidbody2D rb;
    AudioSource audioSource;
    public float speed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb.MovePosition(rb.position + movement.normalized * speed *Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
       if (other.tag== "Coletavel")
       {
        if (audioSource != null) audioSource.Play();
        GameController.Collect();

        Destroy(other.gameObject);
       }
    }
}
