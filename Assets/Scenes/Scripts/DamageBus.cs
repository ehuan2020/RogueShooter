using UnityEngine;

// Central apply-damage path. Everything (weapon, gods, specials) routes through here
// so debuffs (Gaze), mastery, and card multipliers can compose cleanly later.
public static class DamageBus
{
    public static void Apply(Enemy enemy, int baseDamage)
    {
        if (enemy == null) return;

        // Later, multiply by enemy.GazeMultiplier, card mults, etc.
        int finalDamage = Mathf.RoundToInt(baseDamage * enemy.GazeMultiplier);
        enemy.TakeDamage(finalDamage);
    }
}