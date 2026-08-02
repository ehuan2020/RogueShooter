using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeCard", menuName = "Game/Upgrade Card")]
public class UpgradeCard : ScriptableObject
{
    [Header("Display")]
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Effect")]
    public UpgradeType type;
    public float amount;   // meaning depends on type (e.g. 0.2 = +20%, or 10 = +10 HP)

    // called when the player picks this card
    public void Apply(PlayerUpgrades player)
    {
        player.ApplyUpgrade(type, amount);
    }
}

public enum UpgradeType
{
    FireRate,       // lower interval = faster; amount = fraction faster (0.2 = 20% faster)
    Damage,         // +amount damage
    MoveSpeed,      // +amount move speed
    MaxHP,          // +amount max HP
    ProjectileCount // +amount extra projectiles (integer-ish)
}