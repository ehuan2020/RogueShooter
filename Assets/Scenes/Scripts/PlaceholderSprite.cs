using UnityEngine;

// Generates a simple filled-circle sprite at runtime for any god visual prefab that
// doesn't have real art yet (SpriteRenderer.sprite == null) - keeps the prefab's own
// tint color, just gives it something visible instead of literally nothing.
public static class PlaceholderSprite
{
    static Sprite cached;

    public static Sprite Circle
    {
        get
        {
            if (cached == null) cached = BuildCircle();
            return cached;
        }
    }

    static Sprite BuildCircle()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;

        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float alpha = Mathf.Clamp01(radius - dist + 1f);   // ~1px soft edge
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
