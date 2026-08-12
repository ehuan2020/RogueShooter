using UnityEngine;

[CreateAssetMenu(fileName = "BossDefinition", menuName = "Numen/Boss Definition")]
public class BossDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;

    [Header("Phases")]
    // fraction of maxHP that ends phase[i] and begins phase[i+1], e.g. {0.66, 0.33} = 3 phases
    public float[] phaseHpThresholds = { 0.66f, 0.33f };
    public BossPhase[] phases;
}

[System.Serializable]
public class BossPhase
{
    public BossAttack[] attacks;
    public float minCastDelay = 2f;
    public float maxCastDelay = 4f;
}
