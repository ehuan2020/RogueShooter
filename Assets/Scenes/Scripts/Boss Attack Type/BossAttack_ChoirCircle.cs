using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "BossAttack_ChoirCircle", menuName = "Numen/Boss Attack/Choir Circle")]
public class BossAttack_ChoirCircle : BossAttack
{
    public float radius = 2.5f;
    public float zoneDuration = 2f;
    public float tickInterval = 0.5f;
    public int tickDamage = 1;
    public Color telegraphColor = Color.magenta;

    public override IEnumerator Execute(BossController boss, Transform player)
    {
        Vector3 center = player.position;   // pinned at cast time, same dodge-the-telegraph read as the beam

        TempLineFlash.SpawnCircle(center, radius, telegraphColor, telegraphDuration);
        yield return new WaitForSeconds(telegraphDuration);

        TempLineFlash.SpawnCircle(center, radius, telegraphColor, zoneDuration);

        float elapsed = 0f;
        float tickTimer = 0f;
        while (elapsed < zoneDuration)
        {
            bool tick = tickTimer <= 0f;
            if (tick)
            {
                if (Vector2.Distance(center, player.position) <= radius)
                {
                    var hp = player.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(tickDamage);
                }
                tickTimer = tickInterval;
            }

            tickTimer -= Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
