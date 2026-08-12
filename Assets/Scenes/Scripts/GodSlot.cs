using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GodSlot : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text nameText;
    public TMP_Text roleText;
    public Button button;
    public GameObject selectedIndicator;   // simple child toggled active while this god is selected

    GodDefinition god;
    bool isSelected;
    Action<GodDefinition, bool> onToggle;

    public void Setup(GodDefinition definition, bool startSelected, Action<GodDefinition, bool> toggleCallback)
    {
        if (definition == null) return;

        god = definition;
        onToggle = toggleCallback;
        isSelected = startSelected;

        if (nameText != null) nameText.text = definition.displayName;
        if (roleText != null) roleText.text = definition.role.ToString();
        if (selectedIndicator != null) selectedIndicator.SetActive(isSelected);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
        else
        {
            Debug.LogError($"[GodSlot] 'button' reference is missing on {gameObject.name}!", this);
        }
    }

    void HandleClick()
    {
        isSelected = !isSelected;
        if (selectedIndicator != null) selectedIndicator.SetActive(isSelected);
        onToggle?.Invoke(god, isSelected);
    }
}
