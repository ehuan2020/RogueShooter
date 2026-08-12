using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "BossAttack_BeamFan", menuName = "Numen/Boss Attack/Beam Fan")]
public class BossAttack_BeamFan : BossAttack
{
    public int beamCount = 5;
    public int gapCount = 1;      // how many of the beamCount slots stay open as dodge gaps
    public float beamLength = 8f;
    public int damage = 2;
    public float hitRadius = 0.6f;
    public Color telegraphColor = Color.red;

    public override IEnumerator Execute(BossController boss, Transform player)
    {
        Vector3 origin = boss.transform.position;

        var isGap = new bool[beamCount];
        int gaps = Mathf.Clamp(gapCount, 0, beamCount);
        for (int i = 0; i < gaps; i++)
        {
            int slot;
            do { slot = Random.Range(0, beamCount); } while (isGap[slot]);
            isGap[slot] = true;
        }

        var ends = new Vector3[beamCount];
        float startAngle = Random.Range(0f, 360f);
        for (int i = 0; i < beamCount; i++)
        {
            if (isGap[i]) continue;
            float angle = startAngle + (360f / beamCount) * i;
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
            ends[i] = origin + dir * beamLength;
            TempLineFlash.Spawn(origin, ends[i], telegraphColor, telegraphDuration);
        }

        yield return new WaitForSeconds(telegraphDuration);

        for (int i = 0; i < beamCount; i++)
        {
            if (isGap[i]) continue;
            if (PointNearSegment(origin, ends[i], player.position, hitRadius))
            {
                var hp = player.GetComponent<PlayerHealth>();
                if (hp != null) hp.TakeDamage(damage);
                break;   // one hit is enough even if caught in a beam overlap
            }
        }
    }
}
