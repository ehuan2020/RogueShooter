using UnityEngine;

public class GodCompanion : MonoBehaviour
{
    public GodDefinition Definition { get; private set; }

    Transform player;
    GodChorus chorus;

    float orbitAngle;          // current angle around player (degrees)
    float attackTimer;
    float cooldownScale = 1f;  // cards can reduce

    GodSpecial specialInstance;
    float specialTimer;        // counts down; 0 = ready

    public void Init(GodDefinition def, GodChorus owner)
    {
        Definition = def;
        chorus = owner;
        player = owner.transform;

        // spawn the visual body the artists made
        if (def.visualPrefab != null)
            Instantiate(def.visualPrefab, transform.position, Quaternion.identity, transform);

        // clone the special asset so per-instance cooldown state doesn't clobber the shared asset
        if (def.special != null)
            specialInstance = Instantiate(def.special);
    }

    public void SetOrbitPhase(float startAngle) => orbitAngle = startAngle;
    public void ScaleSpecialCooldown(float multiplier) => cooldownScale *= multiplier;

    void Update()
    {
        Orbit();
        AutoAttack();
        TickSpecialCooldown();
    }

    void Orbit()
    {
        orbitAngle += Definition.orbitSpeed * chorus.baseOrbitSpeedMultiplier * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * Definition.orbitRadius;
        transform.position = player.position + offset;
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
            case GodAutoAttackType.Aura:
                // aura is passive/continuous; handled separately, no timed attack
                break;
        }

        attackTimer = Definition.autoAttackInterval;
    }

    void FireProjectile(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        var proj = Instantiate(Definition.autoAttackProjectile, transform.position, Quaternion.identity);
        var p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage = Definition.autoAttackDamage;
            p.Launch(dir);
        }
    }

    void MeleeSnap(Transform target)
    {
        // simple version: if target is in melee range, damage it directly through the bus
        if (Vector2.Distance(transform.position, target.position) <= Definition.autoAttackRange)
        {
            var enemy = target.GetComponent<Enemy>();
            if (enemy != null)
                DamageBus.Apply(enemy, Definition.autoAttackDamage);
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
        return true;
    }

    public float SpecialCooldownNormalized =>
        Definition.specialCooldown <= 0 ? 1f
        : 1f - Mathf.Clamp01(specialTimer / (Definition.specialCooldown * cooldownScale));

    Transform FindNearestEnemy(float range)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float best = range * range;
        foreach (var e in enemies)
        {
            float d = (e.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; nearest = e.transform; }
        }
        return nearest;
    }
}