using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Special_SilentJudgment", menuName = "Numen/Specials/Silent Judgment")]
public class SilentJudgmentSpecial : GodSpecial
{
    public float radius = 4f;
    public float duration = 3f;
    public float slowMultiplier = 0.3f;
    public int tickDamage = 3;
    public float tickInterval = 0.5f;

    public override void Activate(GodCompanion god, Transform player)
    {
        TempLineFlash.SpawnCircle(player.position, radius, Color.magenta, duration);
        god.StartCoroutine(StasisRoutine(god, player));
    }

    IEnumerator StasisRoutine(GodCompanion god, Transform player)
    {
        float elapsed = 0f;
        float tickTimer = 0f;

        while (elapsed < duration)
        {
            bool tick = tickTimer <= 0f;

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
            {
                var enemy = e.GetComponent<Enemy>();
                if (enemy == null) continue;
                if (Vector2.Distance(player.position, enemy.transform.position) > radius) continue;

                enemy.SetSlow(slowMultiplier, 0.25f);
                if (tick) DamageBus.Apply(enemy, tickDamage);
            }

            if (tick) tickTimer = tickInterval;
            tickTimer -= Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
