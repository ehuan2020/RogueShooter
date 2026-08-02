using UnityEngine;

public class XPGem : MonoBehaviour
{
    public int xpValue = 1;
    public float magnetRange = 2.5f;   // starts flying to player within this range
    public float magnetSpeed = 10f;

    Transform player;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // magnet: when player is close, gem flies toward them
        if (dist < magnetRange)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, player.position, magnetSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            XPManager.Instance.AddXP(xpValue);
            Destroy(gameObject);
        }
    }
}