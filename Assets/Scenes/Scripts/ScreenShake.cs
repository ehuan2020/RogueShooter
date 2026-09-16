using UnityEngine;

// Static trigger, same pattern as TempLineFlash - callable from anywhere
// (player hit, boss phase change, specials) without a wired scene reference.
public static class ScreenShake
{
    static CameraFollow cached;

    static CameraFollow Cam
    {
        get
        {
            if (cached == null) cached = Object.FindFirstObjectByType<CameraFollow>();
            return cached;
        }
    }

    public static void Trigger(float duration, float magnitude)
    {
        var cam = Cam;
        if (cam != null) cam.Shake(duration, magnitude);
    }
}
