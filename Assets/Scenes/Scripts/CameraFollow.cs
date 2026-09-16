using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // drag the player here
    public float smoothTime = 0.15f;  // higher = laggier/softer follow
    public Vector2 offset = Vector2.zero;

    Vector3 velocity;

    float shakeTimer;
    float shakeDuration;
    float shakeMagnitude;

    void LateUpdate()   // LateUpdate so it follows AFTER the player has moved this frame
    {
        if (target == null) return;

        Vector3 goal = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z);      // keep camera's own Z (important for 2D)

        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);

        if (shakeTimer > 0f)
        {
            // unscaled: Time.deltaTime is 0 while paused (Time.timeScale = 0, e.g. the
            // death shake during the game-over freeze), which would otherwise stall the
            // countdown forever while still adding a fresh random offset every frame
            shakeTimer -= Time.unscaledDeltaTime;
            float t = shakeDuration > 0f ? Mathf.Clamp01(shakeTimer / shakeDuration) : 0f;
            transform.position += (Vector3)(Random.insideUnitCircle * shakeMagnitude * t);
        }
    }

    public void Shake(float duration, float magnitude)
    {
        // a new shake only overrides a weaker one already in progress, so a big hit
        // isn't swallowed by the tail end of a small one
        if (shakeTimer > 0f && magnitude < shakeMagnitude) return;

        shakeTimer = duration;
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}