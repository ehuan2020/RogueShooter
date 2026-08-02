using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // drag the player here
    public float smoothTime = 0.15f;  // higher = laggier/softer follow
    public Vector2 offset = Vector2.zero;

    Vector3 velocity;

    void LateUpdate()   // LateUpdate so it follows AFTER the player has moved this frame
    {
        if (target == null) return;

        Vector3 goal = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z);      // keep camera's own Z (important for 2D)

        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }
}