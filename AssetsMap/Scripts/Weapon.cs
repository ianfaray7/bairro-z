using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform muzzlePoint;
    public float projectileSpeed = 12f;
    public float localCooldown = 0.0f;
    float cooldownTimer = 0f;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;
    }

    public void Fire(Vector2 targetWorldPos)
    {
        if (cooldownTimer > 0f) return;

        Vector2 origin = muzzlePoint ? (Vector2)muzzlePoint.position : (Vector2)transform.position;
        Vector2 dir = (targetWorldPos - origin).normalized;

        GameObject p = Instantiate(projectilePrefab, origin, Quaternion.identity);
        Projectile proj = p.GetComponent<Projectile>();
        if (proj != null)
            proj.Launch(dir * projectileSpeed);

        cooldownTimer = localCooldown;
    }
}
