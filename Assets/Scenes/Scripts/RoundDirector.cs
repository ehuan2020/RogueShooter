using System.Collections;
using UnityEngine;
using System;

public class RoundDirector : MonoBehaviour
{
    [Header("Round Config")]
    public int totalRounds = 20;
    public float roundDuration = 30f;        // seconds of fodder per normal round
    public int minibossRound = 10;
    public int bossRound = 20;

    [Header("Spawning (fodder)")]
    public GameObject[] enemyPrefabs;
    public float baseSpawnInterval = 1.5f;   // at round 1
    public float minSpawnInterval = 0.3f;    // at final round
    public int baseEnemiesPerSpawn = 1;
    public int maxEnemiesPerSpawn = 4;
    public float spawnRadius = 12f;
    public float spawnRadiusJitter = 2f;

    [Header("Bosses")]
    public GameObject minibossPrefab;
    public GameObject bossPrefab;

    [Header("Refs")]
    public GameOverScreen winScreen;         // reuse your screen, or a dedicated win screen

    public int CurrentRound { get; private set; }
    public event Action<int> OnRoundChanged;     // for the round-counter UI
    public event Action OnVictory;

    Transform player;
    bool running;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        StartCoroutine(RunGame());
    }

    IEnumerator RunGame()
    {
        running = true;

        for (int round = 1; round <= totalRounds; round++)
        {
            CurrentRound = round;
            OnRoundChanged?.Invoke(round);

            if (round == bossRound)
                yield return StartCoroutine(BossRound(bossPrefab));
            else if (round == minibossRound)
                yield return StartCoroutine(BossRound(minibossPrefab));
            else
                yield return StartCoroutine(FodderRound(round));
        }

        // cleared round 20
        Victory();
    }

    IEnumerator FodderRound(int round)
    {
        float t = (float)(round - 1) / (totalRounds - 1);   // 0..1 difficulty ramp
        float interval = Mathf.Lerp(baseSpawnInterval, minSpawnInterval, t);
        int perSpawn = Mathf.RoundToInt(Mathf.Lerp(baseEnemiesPerSpawn, maxEnemiesPerSpawn, t));

        float roundTimer = roundDuration;
        float spawnTimer = 0f;

        while (roundTimer > 0f)
        {
            roundTimer -= Time.deltaTime;
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                for (int i = 0; i < perSpawn; i++) SpawnFodder();
                spawnTimer = interval;
            }
            yield return null;
        }
        // (optional) small gap between rounds
        yield return new WaitForSeconds(1f);
    }

    IEnumerator BossRound(GameObject bossPrefab)
    {
        if (bossPrefab == null || player == null) yield break;

        // spawn boss and wait until it's destroyed
        var boss = Instantiate(bossPrefab, player.position + Vector3.up * 6f, Quaternion.identity);
        while (boss != null)
            yield return null;   // wait for the boss GameObject to be destroyed

        yield return new WaitForSeconds(1f);
    }

    void SpawnFodder()
    {
        if (enemyPrefabs.Length == 0 || player == null) return;

        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float dist = spawnRadius + UnityEngine.Random.Range(-spawnRadiusJitter, spawnRadiusJitter);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        Vector3 pos = player.position + (Vector3)offset;

        var prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
        Instantiate(prefab, pos, Quaternion.identity);
    }

    void Victory()
    {
        running = false;
        Time.timeScale = 0f;
        OnVictory?.Invoke();

        if (winScreen != null)
        {
            var gm = GameManager.Instance;
            float time = gm != null ? gm.timeSurvived : 0f;
            int kills = gm != null ? gm.enemiesKilled : 0;
            int level = (gm != null && gm.xpManager != null) ? gm.xpManager.currentLevel : 1;
            winScreen.Show(time, level, kills);
        }
    }
}