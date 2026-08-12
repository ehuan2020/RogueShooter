using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    PlayerMovement movement;
    PlayerShooter shooter;
    PlayerHealth health;
    GodChorus chorus;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        shooter = GetComponent<PlayerShooter>();
        health = GetComponent<PlayerHealth>();
        chorus = GetComponent<GodChorus>();
    }

    public void ApplyUpgrade(UpgradeType type, float amount)
    {
        switch (type)
        {
            case UpgradeType.FireRate:
                shooter.autoFireInterval = Mathf.Max(0.05f, shooter.autoFireInterval * (1f - amount));
                shooter.manualFireInterval = Mathf.Max(0.05f, shooter.manualFireInterval * (1f - amount));
                break;

            case UpgradeType.Damage:
                // bump the damage on future projectiles (see note below)
                shooter.bonusDamage += Mathf.RoundToInt(amount);
                break;

            case UpgradeType.MoveSpeed:
                movement.moveSpeed += amount;
                break;

            case UpgradeType.MaxHP:
                health.IncreaseMaxHP(Mathf.RoundToInt(amount));
                break;

            case UpgradeType.ProjectileCount:
                shooter.extraProjectiles += Mathf.RoundToInt(amount);
                break;

            case UpgradeType.GodCooldownReduction:
                chorus.ModifyAllCooldowns(1f - amount);
                break;

            case UpgradeType.GodOrbitSpeed:
                chorus.ModifyOrbitSpeed(1f + amount);
                break;

            case UpgradeType.DuplicateGod:
                chorus.DuplicateRandomGod();
                break;
        }
    }
}