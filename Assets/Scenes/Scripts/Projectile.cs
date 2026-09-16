using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float speed = 14f;
    public int damage = 1;
    public float lifetime = 3f;   // auto-destroy so bullets don't pile up

    public GameObject impactPrefab;
    public Sprite[] bulletFrames;   // optional looping flipbook in place of a static sprite

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (bulletFrames != null && bulletFrames.Length > 0)
        {
            var anim = gameObject.AddComponent<FrameAnimator>();
            anim.frames = bulletFrames;
            anim.framesPerSecond = 30f;
            anim.loop = true;
            anim.destroyOnFinish = false;
        }
    }

    // called by the shooter right after spawning, sets travel direction
    public void Launch(Vector2 direction)
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;

        // rotate sprite to face travel direction (optional, looks nice)
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null) DamageBus.Apply(enemy, damage);
            if (impactPrefab != null) Instantiate(impactPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);   // bullet dies on hit (remove this line for piercing)
        }
    }
}