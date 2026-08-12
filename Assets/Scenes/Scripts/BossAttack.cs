using UnityEngine;
using System.Collections;

public abstract class BossAttack : ScriptableObject
{
    public float telegraphDuration = 0.8f;

    public abstract IEnumerator Execute(BossController boss, Transform player);

    // shared hit-test for beam-style attacks: is `point` within `hitRadius` of the segment from -> to
    protected static bool PointNearSegment(Vector3 from, Vector3 to, Vector3 point, float hitRadius)
    {
        Vector2 a = from;
        Vector2 b = to;
        Vector2 p = point;

        float sqrLen = (b - a).sqrMagnitude;
        float t = sqrLen > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, b - a) / sqrLen) : 0f;
        Vector2 closest = a + t * (b - a);

        return (p - closest).sqrMagnitude <= hitRadius * hitRadius;
    }
}
