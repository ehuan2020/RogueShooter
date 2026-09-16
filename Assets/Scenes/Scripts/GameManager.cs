using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Refs")]
    public PlayerHealth playerHealth;
    public XPManager xpManager;
    public GameOverScreen gameOverScreen;

    [Header("Run Stats (tracked live)")]
    public int enemiesKilled = 0;
    public float timeSurvived = 0f;

    bool isGameOver = false;

    void Awake() => Instance = this;

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDeath;
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDeath;
    }

    void Update()
    {
        if (!isGameOver)
            timeSurvived += Time.deltaTime;
    }

    // enemies call this when they die (see note below)
    public void RegisterKill()
    {
        enemiesKilled++;
    }

    void HandlePlayerDeath()
    {
        if (isGameOver) return;
        isGameOver = true;

        Time.timeScale = 0f;   // freeze everything

        int coinsEarned = enemiesKilled;   // 1 coin/kill for now - simple to retune later
        SaveManager.AddCoins(coinsEarned);
        SaveManager.Save();

        int level = (xpManager != null) ? xpManager.currentLevel : 1;
        gameOverScreen.Show(timeSurvived, level, enemiesKilled, coinsEarned);
    }
}