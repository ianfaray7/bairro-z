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
        var go = other.gameObject;
        bool isEnemy = (go.layer == LayerMask.NameToLayer("Enemy") || go.CompareTag("Enemy"));
        if (isEnemy)
        {
            // Tenta aplicar dano em qualquer componente que tenha TakeDamage(float)
            other.gameObject.SendMessage("TakeDamage", (float)damage, SendMessageOptions.DontRequireReceiver);
            // Toca som de hit se houver um MapAudioManager
            if (MapAudioManager.main != null) MapAudioManager.main.PlayZombieHit();
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
