using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2.5f;
    public int maxHP = 3;
    public int contactDamage = 1;
    public float contactCooldown = 0.5f;   // seconds between hits on the player

    [Header("Drops")]
    public GameObject xpGemPrefab;         // spawned on death; leave empty for now

    Rigidbody2D rb;
    Transform player;
    int hp;
    float contactTimer;

    Animator animator;
    bool isDying = false;

    public SpriteRenderer sr;          // drag the sprite (or auto-get in Awake)
    public float flashDuration = 0.08f;
    Color originalColor;

    void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        hp = maxHP;
    }

    void Start()
    {
        // find the player once. Player must have tag "Player".
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // ---- DIRECT SEEK: move straight toward the player ----
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        // optional: flip sprite to face travel direction
        if (dir.x != 0)
        {
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * Mathf.Sign(dir.x);
            transform.localScale = s;
        }
    }

    void Update()
    {
        if (contactTimer > 0f) contactTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (isDying) return;

        hp -= amount;
        if (sr != null) StartCoroutine(HitFlash());

        if (hp <= 0) Die();
    }

    IEnumerator HitFlash()
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
    }

    void Die()
    {
        if (isDying) return;
        isDying = true;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterKill();

        if (xpGemPrefab != null)
            Instantiate(xpGemPrefab, transform.position, Quaternion.identity);

        // stop moving and stop colliding during the death animation
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Death");
            Destroy(gameObject, 0.5f);   // match your death clip's length
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // deal damage while touching the player, on a cooldown so it doesn't drain instantly
    void OnCollisionStay2D(Collision2D other)
    {
        if (contactTimer > 0f) return;
        if (other.gameObject.CompareTag("Player"))
        {
            var hpComp = other.gameObject.GetComponent<PlayerHealth>();
            if (hpComp != null)
            {
                hpComp.TakeDamage(contactDamage);
                contactTimer = contactCooldown;
            }
        }
    }
}