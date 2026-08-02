using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Floating Bob (visual only)")]
    public Transform spriteChild;
    public float bobAmount = 0.15f;
    public float bobSpeed = 2f;

    Rigidbody2D rb;
    Vector2 moveInput;
    Vector3 spriteBaseLocalPos;

    PlayerControls controls;   // the generated class from your Input Actions asset

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (spriteChild != null)
            spriteBaseLocalPos = spriteChild.localPosition;

        controls = new PlayerControls();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        // one read, works for WASD or gamepad stick automatically
        moveInput = controls.Player.Move.ReadValue<Vector2>();

        // guard against slight stick drift / over-length diagonals
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        if (spriteChild != null)
        {
            float offset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            spriteChild.localPosition = spriteBaseLocalPos + Vector3.up * offset;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}