using UnityEngine;
using System;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance;   // simple singleton so gems can find it

    [Header("Leveling")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5;       // XP needed for the first level-up
    public float levelGrowth = 1.4f;    // each level needs ~40% more XP

    [Header("Refs")]
    public WaveManager waveManager;     // calls ShowUpgradeChoices() on level-up

    // events for UI (XP bar, level text)
    public event Action<int, int> OnXPChanged;   // (currentXP, xpToNextLevel)
    public event Action<int> OnLevelUp;          // (newLevel)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        // handle possibly leveling up multiple times from one big gain
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }

        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
    }

    void LevelUp()
    {
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * levelGrowth);

        OnLevelUp?.Invoke(currentLevel);

        // trigger the card screen
        if (waveManager != null)
            waveManager.ShowUpgradeChoices();
    }
}