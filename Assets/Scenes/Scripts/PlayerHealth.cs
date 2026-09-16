using System;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 10;

    [Header("I-Frames")]
    public float invulnTime = 0.3f;   // brief invulnerability after a hit

    [Header("Optional: sprite to flash on hit")]
    public SpriteRenderer sprite;     // drag the sprite child; leave empty to skip flash

    int currentHP;
    float invulnTimer;
    bool isDead;

    // events other scripts (UI, GameManager) can listen to
    public event Action<int, int> OnHealthChanged;   // (current, max)
    public event Action OnDied;

    void Awake()
    {
        currentHP = maxHP;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);   // let UI init
    }

    void Update()
    {
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (isDead || invulnTimer > 0f) return;

        currentHP -= amount;
        invulnTimer = invulnTime;
        OnHealthChanged?.Invoke(currentHP, maxHP);
        ScreenShake.Trigger(0.15f, 0.15f);

        if (sprite != null) StartCoroutine(FlashRed());

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    // upgrade cards can call this to raise max HP
    public void IncreaseMaxHP(int amount, bool healToo = true)
    {
        maxHP += amount;
        if (healToo) currentHP += amount;
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    void Die()
    {
        isDead = true;
        ScreenShake.Trigger(0.4f, 0.35f);
        OnDied?.Invoke();
        // for now, just stop the player. GameManager will handle the game-over screen later.
        Debug.Log("Player died");
        // Time.timeScale = 0f;   // uncomment to freeze the game on death
    }

    System.Collections.IEnumerator FlashRed()
    {
        Color original = sprite.color;
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        sprite.color = original;
    }
}