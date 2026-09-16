using System.Collections.Generic;

// Plain data, matches the SaveState contract in CLAUDE.md. JsonUtility can't
// serialize Dictionary, so godMastery is a list of entries instead.
[System.Serializable]
public class SaveState
{
    public int coins;
    public int bossTokens;

    public List<string> ownedGodIds = new List<string>();
    public List<string> ownedWeaponIds = new List<string>();   // no Weapon system yet - reserved for when one exists
    public List<MasteryEntry> godMastery = new List<MasteryEntry>();

    public List<string> equippedGodIds = new List<string>();
    public string equippedWeaponId;                            // reserved, see ownedWeaponIds
    public int slotCount = 3;
    public int ascensionTier;
}

[System.Serializable]
public class MasteryEntry
{
    public string godId;
    public int level;
}
