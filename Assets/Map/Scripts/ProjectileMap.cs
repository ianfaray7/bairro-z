using UnityEngine;

public class ProjectileMap : MonoBehaviour
{
    public float lifeTime = 4f;
    Rigidbody2D rb;
    public int damage = 1;
    public bool destroyOnHit = true;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    public void Launch(Vector2 velocity)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = velocity;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // exemplo simples: se colidir com "Enemy" aplicar dano
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // se tiver script de inimigo, aplique dano aqui
            // var e = other.GetComponent<Enemy>(); if (e) e.TakeDamage(damage);
            if (destroyOnHit) Destroy(gameObject);
        }
        else
        {
            // colidir com cenário (opcional)
            // Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // caso use colisões físicas
        if (destroyOnHit) Destroy(gameObject);
    }
}
