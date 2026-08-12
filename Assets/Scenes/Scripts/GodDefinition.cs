using UnityEngine;

public enum GodRole { SingleTarget, SwarmClear, Control, Support, Dps }
public enum UnlockType { Coin, Token }

[CreateAssetMenu(fileName = "God", menuName = "Numen/God Definition")]
public class GodDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;                 // stable key for save data, e.g. "eclipsed_eye"
    public string displayName;
    public GodRole role;
    public UnlockType unlockType;
    public int cost;

    [Header("Orbit")]
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f;    // degrees/sec around the player

    [Header("Auto-Attack")]
    public float autoAttackRange = 5f;
    public float autoAttackInterval = 1f;
    public int autoAttackDamage = 2;
    public GameObject autoAttackProjectile;   // null = melee/aura god (handled by behaviour)
    public GodAutoAttackType autoAttackType = GodAutoAttackType.Projectile;
    public float auraSlowMultiplier = 0.5f;   // only used when autoAttackType == Aura

    [Header("Special")]
    public GodSpecial special;        // a GodSpecial asset (see below) � the triggered ability
    public float specialCooldown = 8f;

    [Header("Visuals")]
    public GameObject visualPrefab;   // the sprite/animated body the artists deliver
}

public enum GodAutoAttackType { Projectile, MeleeSnap, Aura }