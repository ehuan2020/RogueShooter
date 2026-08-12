using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Special_RendingRush", menuName = "Numen/Specials/Rending Rush")]
public class RendingRushSpecial : GodSpecial
{
    public float scanRadius = 6f;
    public float clusterRadius = 2f;    // how close enemies must be to each other to count as "the same group"
    public float dashSpeed = 20f;
    public float hitRadius = 1f;
    public int dashDamage = 4;
    public int executeThresholdHP = 5;  // enemies at or below this HP are deleted outright, not just damaged
    public float knockbackSpeed = 12f;
    public float knockbackDuration = 0.2f;

    public override void Activate(GodCompanion god, Transform player)
    {
        if (!TryFindDensestPoint(player.position, out Vector3 target)) return;
        god.StartCoroutine(DashRoutine(god, god.transform.position, target));
    }

    IEnumerator DashRoutine(GodCompanion god, Vector3 origin, Vector3 target)
    {
        god.SetDashing(true);
        yield return Dash(god, target);
        yield return Dash(god, origin);
        god.SetDashing(false);
    }

    IEnumerator Dash(GodCompanion god, Vector3 destination)
    {
        Vector3 from = god.transform.position;
        float duration = Vector3.Distance(from, destination) / dashSpeed;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            god.transform.position = Vector3.Lerp(from, destination, duration > 0f ? t / duration : 1f);
            HitAlongPath(god.transform.position);
            yield return null;
        }
        god.transform.position = destination;
    }

    void HitAlongPath(Vector3 point)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            if (Vector2.Distance(point, e.transform.position) > hitRadius) continue;

            var enemy = e.GetComponent<Enemy>();
            if (enemy == null) continue;

            if (enemy.CurrentHP <= executeThresholdHP)
                DamageBus.Apply(enemy, enemy.CurrentHP);   // execute low-HP fodder outright
            else
                DamageBus.Apply(enemy, dashDamage);

            Vector2 dir = ((Vector2)e.transform.position - (Vector2)point);
            dir = dir.sqrMagnitude > 0.001f ? dir.normalized : Random.insideUnitCircle.normalized;
            enemy.ApplyKnockback(dir * knockbackSpeed, knockbackDuration);
        }
    }

    // finds the enemy with the most neighbors within clusterRadius, inside scanRadius of center
    bool TryFindDensestPoint(Vector3 center, out Vector3 point)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        point = center;
        int bestCount = 0;

        foreach (var e in enemies)
        {
            if (Vector2.Distance(center, e.transform.position) > scanRadius) continue;

            int count = 0;
            foreach (var other in enemies)
            {
                if (Vector2.Distance(e.transform.position, other.transform.position) <= clusterRadius)
                    count++;
            }

            if (count > bestCount)
            {
                bestCount = count;
                point = e.transform.position;
            }
        }

        return bestCount > 0;
    }
}
