using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Card Pool (drag all your UpgradeCard assets here)")]
    public List<UpgradeCard> allCards;

    [Header("Refs")]
    public CardScreen cardScreen;       // the UI script below
    public PlayerUpgrades player;

    // call this whenever the player levels up
    public void ShowUpgradeChoices()
    {
        Time.timeScale = 0f;                       // freeze the game
        List<UpgradeCard> choices = PickThree();
        cardScreen.Show(choices, OnCardPicked);
    }

    List<UpgradeCard> PickThree()
    {
        // copy the pool and pull 3 distinct random cards
        List<UpgradeCard> pool = new List<UpgradeCard>(allCards);
        List<UpgradeCard> picks = new List<UpgradeCard>();
        int n = Mathf.Min(3, pool.Count);
        for (int i = 0; i < n; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            picks.Add(pool[idx]);
            pool.RemoveAt(idx);                    // no duplicates in the same offer
        }
        return picks;
    }

    void OnCardPicked(UpgradeCard card)
    {
        card.Apply(player);                        // run the effect
        cardScreen.Hide();
        Time.timeScale = 1f;                       // unfreeze, back to the fight
    }
}