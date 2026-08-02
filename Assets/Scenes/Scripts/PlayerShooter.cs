using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Refs")]
    public GameObject projectilePrefab;
    public Transform firePoint;        // empty child at the player's center/muzzle; defaults to self

    [Header("Fire Rates (seconds between shots)")]
    public float autoFireInterval = 0.6f;   // upgrade cards can lower this
    public float manualFireInterval = 0.25f;

    [Header("Auto-aim")]
    public float autoAimRange = 8f;    // only auto-target enemies within this radius

    [Header("Upgrades & Stats")]
    public int bonusDamage = 0;        // added to projectile base damage
    public int extraProjectiles = 0;   // extra shots per attack
    public float spreadAngle = 15f;    // angle offset between spread shots (in degrees)

    PlayerControls controls;
    Camera cam;
    float autoTimer;
    float manualTimer;

    void Awake()
    {
        controls = new PlayerControls();
        cam = Camera.main;
        if (firePoint == null) firePoint = transform;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        autoTimer -= Time.deltaTime;
        manualTimer -= Time.deltaTime;

        // ---- AUTO FIRE: shoot nearest enemy on cooldown ----
        if (autoTimer <= 0f)
        {
            Transform target = FindNearestEnemy();
            if (target != null)
            {
                Vector2 dir = (target.position - firePoint.position);
                Fire(dir);
                autoTimer = autoFireInterval;
            }
        }

        // ---- MANUAL FIRE: player holds Fire, shoots toward Aim ----
        bool firePressed = controls.Player.Fire.IsPressed();
        if (firePressed && manualTimer <= 0f)
        {
            Vector2 dir = GetAimDirection();
            if (dir.sqrMagnitude > 0.001f)
            {
                Fire(dir);
                manualTimer = manualFireInterval;
            }
        }
    }

    void Fire(Vector2 direction)
    {
        int totalProjectiles = 1 + extraProjectiles;
        Vector2 baseDir = direction.normalized;

        // Calculate starting angle offset so spread centers around aiming direction
        float startAngle = -spreadAngle * (totalProjectiles - 1) / 2f;

        for (int i = 0; i < totalProjectiles; i++)
        {
            float currentAngle = startAngle + (i * spreadAngle);
            Vector2 rotatedDir = Quaternion.Euler(0, 0, currentAngle) * baseDir;

            var projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            var proj = projObj.GetComponent<Projectile>();

            if (proj != null)
            {
                // Add bonus damage to base damage
                proj.damage += bonusDamage;
                proj.Launch(rotatedDir);
            }
        }
    }

    // Aim works for BOTH mouse and gamepad stick:
    Vector2 GetAimDirection()
    {
        Vector2 aim = controls.Player.Aim.ReadValue<Vector2>();

        // Heuristic: mouse gives a large screen-position value; stick gives a small (-1..1) vector.
        if (aim.magnitude > 1.5f)
        {
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(aim.x, aim.y, 0f));
            return (Vector2)(world - firePoint.position);
        }

        return aim;
    }

    Transform FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float best = autoAimRange * autoAimRange;

        foreach (var e in enemies)
        {
            float d = (e.transform.position - firePoint.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                nearest = e.transform;
            }
        }
        return nearest;
    }
}