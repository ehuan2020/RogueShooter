using System;
using System.IO;
using UnityEngine;

// Static, not a MonoBehaviour - no scene wiring needed. First access lazily
// loads from disk (or creates a default save); Save() writes it back out.
public static class SaveManager
{
    const string FileName = "numen_save.json";

    static SaveState cached;

    public static SaveState Current
    {
        get
        {
            if (cached == null) Load();
            return cached;
        }
    }

    static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void Load()
    {
        if (File.Exists(FilePath))
        {
            try
            {
                cached = JsonUtility.FromJson<SaveState>(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to read save file, starting fresh: {e.Message}");
            }
        }

        if (cached == null)
            cached = CreateDefault();
    }

    public static void Save()
    {
        if (cached == null) return;
        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(cached, true));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to write save file: {e.Message}");
        }
    }

    // The 3 MVP gods ship unlocked so existing behavior (everything available)
    // doesn't regress for players with no save file yet.
    static SaveState CreateDefault()
    {
        var state = new SaveState { slotCount = 3 };
        state.ownedGodIds.AddRange(new[] { "eclipsed_eye", "veiled_monarch", "visceral_maw" });
        state.equippedGodIds.AddRange(state.ownedGodIds);
        return state;
    }

    public static bool OwnsGod(string godId) => Current.ownedGodIds.Contains(godId);

    public static void UnlockGod(string godId)
    {
        if (!Current.ownedGodIds.Contains(godId))
            Current.ownedGodIds.Add(godId);
    }

    public static int GetMastery(string godId)
    {
        var entry = Current.godMastery.Find(m => m.godId == godId);
        return entry != null ? entry.level : 0;
    }

    public static void AddCoins(int amount) => Current.coins += amount;
    public static void AddBossTokens(int amount) => Current.bossTokens += amount;

    // cost to go from level N-1 to level N; index 0 = cost of the 1st level.
    // "a maxed god is ~30% stronger, never a wall" - 3 levels x 10% keeps that pillar exact.
    public static readonly int[] MasteryLevelCosts = { 100, 250, 500 };
    public const float MasteryDamageBonusPerLevel = 0.10f;
    public const float MasteryCooldownReductionPerLevel = 0.10f;

    public static int MaxMasteryLevel => MasteryLevelCosts.Length;

    // -1 = already maxed
    public static int GetMasteryCost(string godId)
    {
        int level = GetMastery(godId);
        return level < MasteryLevelCosts.Length ? MasteryLevelCosts[level] : -1;
    }

    public static bool TryUpgradeMastery(string godId)
    {
        int level = GetMastery(godId);
        if (level >= MaxMasteryLevel) return false;

        int cost = MasteryLevelCosts[level];
        if (Current.coins < cost) return false;

        Current.coins -= cost;
        var entry = Current.godMastery.Find(m => m.godId == godId);
        if (entry == null)
        {
            entry = new MasteryEntry { godId = godId, level = 0 };
            Current.godMastery.Add(entry);
        }
        entry.level++;
        Save();
        return true;
    }

    public static float MasteryDamageMultiplier(string godId) => 1f + GetMastery(godId) * MasteryDamageBonusPerLevel;
    public static float MasteryCooldownMultiplier(string godId) => 1f - GetMastery(godId) * MasteryCooldownReductionPerLevel;
}
