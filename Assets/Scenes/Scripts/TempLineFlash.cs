using UnityEngine;

// throwaway placeholder VFX so specials have some visible feedback before real art exists
public static class TempLineFlash
{
    public static void Spawn(Vector3 from, Vector3 to, Color color, float duration)
    {
        var go = new GameObject("TempLineFlash");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.startWidth = 0.1f;
        lr.endWidth = 0.03f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = 10;
        Object.Destroy(go, duration);
    }

    public static void SpawnCircle(Vector3 center, float radius, Color color, float duration)
    {
        const int segments = 32;
        var go = new GameObject("TempCircleFlash");
        go.transform.position = center;
        var lr = go.AddComponent<LineRenderer>();
        lr.loop = true;
        lr.positionCount = segments;
        lr.useWorldSpace = false;
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
        lr.startWidth = 0.06f;
        lr.endWidth = 0.06f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = 10;
        Object.Destroy(go, duration);
    }
}
