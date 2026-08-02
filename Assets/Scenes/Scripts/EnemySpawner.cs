using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("What to spawn")]
    public GameObject[] enemyPrefabs;      // one or more enemy types

    [Header("Spawn timing")]
    public float startInterval = 1.5f;     // seconds between spawns at game start
    public float minInterval = 0.2f;       // fastest it ever gets
    public float rampDuration = 180f;      // how long (sec) to reach minInterval

    [Header("Spawn amount ramp")]
    public int startPerSpawn = 1;          // enemies per spawn at start
    public int maxPerSpawn = 5;            // enemies per spawn at peak

    [Header("Where to spawn")]
    public float spawnRadius = 12f;        // distance from player to spawn (just off-screen)
    public float spawnRadiusJitter = 2f;   // random variation in distance

    Transform player;
    float timer;
    float elapsed;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        timer = startInterval;
    }

    void Update()
    {
        if (player == null || enemyPrefabs.Length == 0) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / rampDuration);   // 0 -> 1 over rampDuration

        // interval shrinks and count grows as the game goes on
        float currentInterval = Mathf.Lerp(startInterval, minInterval, t);
        int currentCount = Mathf.RoundToInt(Mathf.Lerp(startPerSpawn, maxPerSpawn, t));

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            for (int i = 0; i < currentCount; i++)
                SpawnOne();
            timer = currentInterval;
        }
    }

    void SpawnOne()
    {
        // pick a random point on a circle around the player (spawns just off-screen)
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float dist = spawnRadius + UnityEngine.Random.Range(-spawnRadiusJitter, spawnRadiusJitter);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        Vector3 pos = player.position + (Vector3)offset;

        var prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
        Instantiate(prefab, pos, Quaternion.identity);
    }
}