using UnityEngine;

public class GodCompanion : MonoBehaviour
{
    public GodDefinition Definition { get; private set; }

    Transform player;
    GodChorus chorus;

    float orbitAngle;          // fixed hover-slot direction around the player (degrees) - never rotates
    float attackTimer;
    float cooldownScale = 1f;  // cards can reduce

    GodSpecial specialInstance;
    float specialTimer;        // counts down; 0 = ready

    // Ryuk-over-the-shoulder float: each god sits at a fixed slot near the player (set
    // once via SetOrbitPhase, clustered top-left by GodChorus) and gently bobs in place
    // while smoothly following - no rotation at all. Attack range is checked against the
    // player, not this transform (see FindNearestEnemy/TickAura/MeleeSnap below) - a
    // literally fixed cosmetic position otherwise leaves whichever side it isn't on
    // undefended, which measurably hurt survival in testing (dying round 0 instead of
    // the normal 5-11).
    [Header("Float")]
    public float bobAmount = 0.15f;
    public float bobSpeed = 2f;
    public float followSmoothTime = 0.25f;

    float bobPhase;
    Vector3 floatVelocity;

    public bool IsDashing { get; private set; }
    public void SetDashing(bool value) => IsDashing = value;

    public void Init(GodDefinition def, GodChorus owner)
    {
        Definition = def;
        chorus = owner;
        player = owner.transform;

        // spawn the visual body the artists made
        if (def.visualPrefab != null)
        {
            var visual = Instantiate(def.visualPrefab, transform.position, Quaternion.identity, transform);
            var sr = visual.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite == null)
                sr.sprite = PlaceholderSprite.Circle;   // no real art yet - keeps the prefab's own tint, just makes it visible
        }

        // clone the special asset so per-instance cooldown state doesn't clobber the shared asset
        if (def.special != null)
            specialInstance = Instantiate(def.special);

        // baseline from mastery; cards further multiply into this via ScaleSpecialCooldown
        cooldownScale = SaveManager.MasteryCooldownMultiplier(def.id);

        bobPhase = Random.Range(0f, Mathf.PI * 2f);   // so multiple gods don't bob in unison
    }

    public void SetOrbitPhase(float startAngle) => orbitAngle = startAngle;
    public void ScaleSpecialCooldown(float multiplier) => cooldownScale *= multiplier;

    void Update()
    {
        Float();
        if (Definition.autoAttackType == GodAutoAttackType.Aura)
            TickAura();
        else
            AutoAttack();
        TickSpecialCooldown();
    }

    void Float()
    {
        if (IsDashing) return;   // special has taken direct control of transform.position

        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 slot = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * Definition.orbitRadius;

        // gentle idle bob so it reads as floating rather than pinned in place; the
        // "orbit speed" card/mastery multiplier now livens up the bob instead of a spin
        bobPhase += bobSpeed * chorus.baseOrbitSpeedMultiplier * Time.deltaTime;
        slot += new Vector3(0f, Mathf.Sin(bobPhase) * bobAmount, 0f);

        Vector3 goal = player.position + slot;
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref floatVelocity, followSmoothTime);
    }

    void AutoAttack()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        Transform target = FindNearestEnemy(Definition.autoAttackRange);
        if (target == null) return;

        switch (Definition.autoAttackType)
        {
            case GodAutoAttackType.Projectile:
                FireProjectile(target);
                break;
            case GodAutoAttackType.MeleeSnap:
                MeleeSnap(target);
                break;
        }

        attackTimer = Definition.autoAttackInterval;
    }

    // continuous slow field centered on the player (not this transform - see the Float
    // header comment); re-evaluated every frame instead of on the attack-interval timer.
    // refreshes a short self-expiring debuff on Enemy rather than tracking enter/exit, so it clears itself
    // automatically once an enemy leaves range or this companion is removed.
    void TickAura()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            var enemy = e.GetComponent<Enemy>();
            if (enemy == null) continue;

            if (Vector2.Distance(player.position, enemy.transform.position) <= Definition.autoAttackRange)
                enemy.SetSlow(Definition.auraSlowMultiplier, 0.25f);
        }
    }

    int EffectiveAutoAttackDamage() =>
        Mathf.RoundToInt(Definition.autoAttackDamage * SaveManager.MasteryDamageMultiplier(Definition.id));

    void FireProjectile(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        var proj = Instantiate(Definition.autoAttackProjectile, transform.position, Quaternion.identity);
        var p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage = EffectiveAutoAttackDamage();
            p.Launch(dir);
        }
    }

    void MeleeSnap(Transform target)
    {
        // range is from the player, not this transform - see the Float header comment
        if (Vector2.Distance(player.position, target.position) <= Definition.autoAttackRange)
        {
            var enemy = target.GetComponent<Enemy>();
            if (enemy != null)
                DamageBus.Apply(enemy, EffectiveAutoAttackDamage());
        }
    }

    void TickSpecialCooldown()
    {
        if (specialTimer > 0f) specialTimer -= Time.deltaTime;
    }

    // called by the input handler when the player triggers THIS god's special
    public bool TryTriggerSpecial()
    {
        if (specialInstance == null || specialTimer > 0f) return false;

        specialInstance.Activate(this, player);
        specialTimer = Definition.specialCooldown * cooldownScale;
        ScreenShake.Trigger(0.2f, 0.2f);   // physical "something happened" cue - the line/ring VFX alone is easy to miss
        return true;
    }

    public float SpecialCooldownNormalized =>
        Definition.specialCooldown <= 0 ? 1f
        : 1f - Mathf.Clamp01(specialTimer / (Definition.specialCooldown * cooldownScale));

    // measured from the player, not this transform - see the Float header comment
    Transform FindNearestEnemy(float range)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float best = range * range;
        foreach (var e in enemies)
        {
            float d = (e.transform.position - player.position).sqrMagnitude;
            if (d < best) { best = d; nearest = e.transform; }
        }
        return nearest;
    }
}