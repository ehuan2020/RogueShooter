using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Enemy))]
public class BossController : MonoBehaviour
{
    public BossDefinition definition;
    public float introDelay = 1.5f;
    public Color phaseChangeColor = Color.white;
    public float phaseChangeRadius = 3f;

    [Header("Visuals")]
    public Sprite idleSprite;
    public Sprite telegraphSprite;

    Enemy enemy;
    Transform player;
    int phaseIndex;
    float attackTimer;
    bool isAttacking;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        enemy.OnDied += HandleDied;
        attackTimer = introDelay;

        if (idleSprite != null && enemy.sr != null) enemy.sr.sprite = idleSprite;
    }

    void OnDestroy()
    {
        if (enemy != null) enemy.OnDied -= HandleDied;
    }

    void Update()
    {
        if (player == null || definition == null || definition.phases == null || definition.phases.Length == 0) return;

        TryAdvancePhase();

        if (isAttacking) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
            StartCoroutine(RunAttack());
    }

    void TryAdvancePhase()
    {
        if (phaseIndex >= definition.phases.Length - 1) return;       // already in the final phase
        if (phaseIndex >= definition.phaseHpThresholds.Length) return;

        float hpFraction = enemy.maxHP > 0 ? (float)enemy.CurrentHP / enemy.maxHP : 0f;
        if (hpFraction > definition.phaseHpThresholds[phaseIndex]) return;

        phaseIndex++;
        TempLineFlash.SpawnCircle(transform.position, phaseChangeRadius, phaseChangeColor, 0.4f);
        ScreenShake.Trigger(0.3f, 0.3f);
        attackTimer = Mathf.Max(attackTimer, 0.75f);   // brief readable beat before the next attack
    }

    IEnumerator RunAttack()
    {
        var phase = definition.phases[phaseIndex];
        if (phase.attacks == null || phase.attacks.Length == 0)
        {
            attackTimer = phase.minCastDelay;
            yield break;
        }

        var attack = phase.attacks[Random.Range(0, phase.attacks.Length)];
        isAttacking = true;

        float storedSpeed = enemy.moveSpeed;
        enemy.moveSpeed = 0f;   // plant during the telegraph/attack so it reads clearly
        if (telegraphSprite != null && enemy.sr != null) enemy.sr.sprite = telegraphSprite;

        yield return StartCoroutine(attack.Execute(this, player));

        enemy.moveSpeed = storedSpeed;
        if (idleSprite != null && enemy.sr != null) enemy.sr.sprite = idleSprite;
        isAttacking = false;
        attackTimer = Random.Range(phase.minCastDelay, phase.maxCastDelay);
    }

    void HandleDied()
    {
        StopAllCoroutines();
        isAttacking = false;
    }
}
