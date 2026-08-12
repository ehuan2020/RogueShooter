using System.Collections.Generic;
using UnityEngine;

public class GodChorus : MonoBehaviour
{
    [Header("Loadout (set by store/loadout; hardcode for testing)")]
    public List<GodDefinition> equippedGods = new List<GodDefinition>();

    [Header("Orbit")]
    public float baseOrbitSpeedMultiplier = 1f;   // cards can raise this

    readonly List<GodCompanion> active = new List<GodCompanion>();

    void Start()
    {
        SpawnLoadout();
    }

    void SpawnLoadout()
    {
        foreach (var def in equippedGods)
            SpawnGod(def);
        RecalculateOrbitSlots();
    }

    // used both at start and by the duplication card
    public void SpawnGod(GodDefinition def)
    {
        var go = new GameObject("God_" + def.id);
        go.transform.SetParent(transform, false);   // parented to player so it follows

        var companion = go.AddComponent<GodCompanion>();
        companion.Init(def, this);

        active.Add(companion);
        RecalculateOrbitSlots();
    }

    // duplication card calls this
    public void DuplicateGod(string godId)
    {
        var def = active.Find(g => g.Definition.id == godId)?.Definition;
        if (def != null) SpawnGod(def);
    }

    // evenly space all active gods around the orbit circle
    void RecalculateOrbitSlots()
    {
        int n = active.Count;
        for (int i = 0; i < n; i++)
            active[i].SetOrbitPhase((360f / n) * i);
    }

    // cards that affect all gods route through here
    public void ModifyAllCooldowns(float multiplier)
    {
        foreach (var g in active) g.ScaleSpecialCooldown(multiplier);
    }

    public void ModifyOrbitSpeed(float multiplier)
    {
        baseOrbitSpeedMultiplier *= multiplier;
    }

    // called by input handling to fire a specific equipped god's special
    public void TriggerSpecial(int index)
    {
        if (index >= 0 && index < active.Count)
            active[index].TryTriggerSpecial();
    }

    public IReadOnlyList<GodCompanion> Active => active;
}