using UnityEngine;

[CreateAssetMenu(fileName = "Special_Gaze", menuName = "Numen/Specials/Gaze of the Void")]
public class GazeSpecial : GodSpecial
{
    public float gazeDuration = 4f;
    public float damageMultiplier = 2f;
    public int beamDamage = 20;

    public override void Activate(GodCompanion god, Transform player)
    {
        // find highest-HP enemy on screen, apply Gaze debuff + heavy hit
        Enemy target = FindHighestHPEnemy();
        if (target == null) return;

        DamageBus.Apply(target, beamDamage);
        target.ApplyGaze(damageMultiplier, gazeDuration);   // debuff hook on Enemy
        TempLineFlash.Spawn(god.transform.position, target.transform.position, Color.cyan, 0.15f);   // placeholder until real beam VFX exists
    }

    Enemy FindHighestHPEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Enemy best = null;
        int most = -1;
        foreach (var e in enemies)
        {
            var en = e.GetComponent<Enemy>();
            if (en != null && en.CurrentHP > most) { most = en.CurrentHP; best = en; }
        }
        return best;
    }
}