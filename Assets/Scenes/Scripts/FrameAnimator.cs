using UnityEngine;

// Plays through a fixed sequence of sprites on a SpriteRenderer. For hand-authored VFX
// frame sequences (muzzle flash, impact, bullet trail) that ship as loose PNGs with no
// baked-in Animator/AnimationClip.
[RequireComponent(typeof(SpriteRenderer))]
public class FrameAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float framesPerSecond = 24f;
    public bool loop = false;
    public bool destroyOnFinish = true;

    SpriteRenderer sr;
    float timer;
    int index;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0) sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);

        while (timer >= frameDuration)
        {
            timer -= frameDuration;
            index++;

            if (index >= frames.Length)
            {
                if (loop)
                {
                    index = 0;
                }
                else
                {
                    index = frames.Length - 1;
                    if (destroyOnFinish) { Destroy(gameObject); return; }
                    enabled = false;
                    return;
                }
            }

            sr.sprite = frames[index];
        }
    }
}
