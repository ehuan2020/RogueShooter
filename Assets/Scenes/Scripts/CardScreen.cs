using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CardScreen : MonoBehaviour
{
    [Header("Root panel to show/hide")]
    public GameObject panel;

    [Header("The 3 card slots (assign in order)")]
    public CardSlot[] slots;   // size 3

    Action<UpgradeCard> onPick;

    void Awake() => panel.SetActive(false);

    public void Show(List<UpgradeCard> cards, Action<UpgradeCard> pickCallback)
    {
        onPick = pickCallback;
        panel.SetActive(true);

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < cards.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Setup(cards[i], HandlePick);
            }
            else slots[i].gameObject.SetActive(false);
        }
    }

    public void Hide() => panel.SetActive(false);

    void HandlePick(UpgradeCard card)
    {
        onPick?.Invoke(card);
    }
}