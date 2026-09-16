using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class GameOverScreen : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text timeText;
    public TMP_Text levelText;
    public TMP_Text killsText;
    public TMP_Text coinsText;
    public Button retryButton;
    public Button quitButton;     // optional

    void Awake()
    {
        panel.SetActive(false);
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
    }

    public void Show(float timeSurvived, int level, int kills, int coinsEarned = 0)
    {
        panel.SetActive(true);

        int minutes = Mathf.FloorToInt(timeSurvived / 60f);
        int seconds = Mathf.FloorToInt(timeSurvived % 60f);
        if (timeText != null) timeText.text = $"Time Survived: {minutes:00}:{seconds:00}";
        if (levelText != null) levelText.text = $"Level Reached: {level}";
        if (killsText != null) killsText.text = $"Enemies Killed: {kills}";
        if (coinsText != null) coinsText.text = $"Coins Earned: {coinsEarned} (Total: {SaveManager.Current.coins})";
    }

    void Retry()
    {
        Time.timeScale = 1f;   // CRITICAL: unfreeze before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Quit()
    {
        Time.timeScale = 1f;
        // for a jam, quit to menu or just quit the app:
        Application.Quit();
        // if you have a main menu scene: SceneManager.LoadScene("MainMenu");
    }
}