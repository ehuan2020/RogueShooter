using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2.5f;
    public int maxHP = 3;
    public int contactDamage = 1;
    public float contactCooldown = 0.5f;   // seconds between hits on the player
    public int CurrentHP => hp;              // public read-only access to hp
    public float GazeMultiplier { get; private set; } = 1f;
    public float SlowMultiplier { get; private set; } = 1f;

    // fired at the start of Die(), before the GameObject is destroyed - lets composed
    // components (e.g. BossController) react to death without needing a virtual Die()
    public event Action OnDied;

    [Header("Drops")]
    public GameObject xpGemPrefab;         // spawned on death; leave empty for now

    Rigidbody2D rb;
    Transform player;
    int hp;
    float contactTimer;
    float gazeTimer;
    float slowTimer;
    float knockbackTimer;
    Vector2 knockbackVelocity;

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

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = knockbackVelocity;
            return;   // being knocked back overrides normal seek this tick
        }

        // ---- DIRECT SEEK: move straight toward the player ----
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed * SlowMultiplier;

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

        // gaze debuff countdown
        if (gazeTimer > 0f)
        {
            gazeTimer -= Time.deltaTime;
            if (gazeTimer <= 0f)
                GazeMultiplier = 1f;   // debuff expires, back to normal
        }

        // slow debuff countdown
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
                SlowMultiplier = 1f;
        }
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

        OnDied?.Invoke();

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterKill();

        if (xpGemPrefab != null)
            Instantiate(xpGemPrefab, transform.position, Quaternion.identity);

        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        if (animator != null && HasParameter("Death", animator))
        {
            animator.SetTrigger("Death");
            Destroy(gameObject, 0.5f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // helper: check if a parameter exists before using it
    bool HasParameter(string paramName, Animator anim)
    {
        foreach (var p in anim.parameters)
            if (p.name == paramName) return true;
        return false;
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
    public void ApplyGaze(float multiplier, float duration)
    {
        GazeMultiplier = multiplier;
        gazeTimer = duration;
    }

    // self-expiring like ApplyGaze; callers refresh this each frame the enemy stays in a slow field
    public void SetSlow(float multiplier, float duration)
    {
        SlowMultiplier = multiplier;
        slowTimer = duration;
    }

    // overrides normal seek movement for a short window so knockback isn't stomped by FixedUpdate's chase logic
    public void ApplyKnockback(Vector2 velocity, float duration)
    {
        knockbackVelocity = velocity;
        knockbackTimer = duration;
    }
}