using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileLuka : MonoBehaviour
{
    public float lifeTime = 3.5f;
    public int damage = 1;
    public bool destroyOnHit = true;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 velocity)
{
    if (rb == null) rb = GetComponent<Rigidbody2D>();
    rb.linearVelocity = velocity;

    // Rotaciona a flecha para a direção do movimento
    float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.Euler(0, 0, angle);

    Destroy(gameObject, lifeTime);
}


void OnTriggerEnter2D(Collider2D other)
{
    // Só atinge objetos com a tag "Enemy"
    if (!other.gameObject.CompareTag("Enemy"))
        return;

    // Aqui futuramente você vai colocar o dano no inimigo
    // Exemplo:
    // var enemy = other.GetComponent<Enemy>();
    // if (enemy != null)
    // {
    //     enemy.TakeDamage(damage);
    // }

    // Destrói o projétil ao atingir um inimigo
    if (destroyOnHit)
        Destroy(gameObject);
}

    void OnCollisionEnter2D(Collision2D col)
    {
        // fallback caso use colisões não-trigger
        if (destroyOnHit) Destroy(gameObject);
    }
}
