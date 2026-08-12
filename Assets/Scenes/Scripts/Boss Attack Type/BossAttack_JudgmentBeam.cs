using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "BossAttack_JudgmentBeam", menuName = "Numen/Boss Attack/Judgment Beam")]
public class BossAttack_JudgmentBeam : BossAttack
{
    public int damage = 2;
    public float hitRadius = 0.6f;
    public Color telegraphColor = Color.red;

    public override IEnumerator Execute(BossController boss, Transform player)
    {
        Vector3 from = boss.transform.position;
        Vector3 to = player.position;   // pinned at cast time - moving off the line dodges it

        TempLineFlash.Spawn(from, to, telegraphColor, telegraphDuration);
        yield return new WaitForSeconds(telegraphDuration);

        if (PointNearSegment(from, to, player.position, hitRadius))
        {
            var hp = player.GetComponent<PlayerHealth>();
            if (hp != null) hp.TakeDamage(damage);
        }
    }
}
