using System;
using TMPro; // <-- Added TextMeshPro namespace
using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text nameText;        // Switched from Text to TMP_Text
    public TMP_Text descriptionText; // Switched from Text to TMP_Text
    public Image iconImage;
    public Button button;

    public void Setup(UpgradeCard card, Action<UpgradeCard> onClick)
    {
        if (card == null) return;

        // Safely set Name
        if (nameText != null)
        {
            nameText.text = card.cardName;
        }
        else
        {
            Debug.LogError($"[CardSlot] 'nameText' reference is missing on {gameObject.name}!", this);
        }

        // Safely set Description
        if (descriptionText != null)
        {
            descriptionText.text = card.description;
        }
        else
        {
            Debug.LogError($"[CardSlot] 'descriptionText' reference is missing on {gameObject.name}!", this);
        }

        // Safely set Icon
        if (iconImage != null)
        {
            if (card.icon != null)
            {
                iconImage.sprite = card.icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                // Hide icon object if the card asset has no image assigned
                iconImage.gameObject.SetActive(false);
            }
        }

        // Safely setup Button click
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(card));
        }
        else
        {
            Debug.LogError($"[CardSlot] 'button' reference is missing on {gameObject.name}!", this);
        }
    }
}