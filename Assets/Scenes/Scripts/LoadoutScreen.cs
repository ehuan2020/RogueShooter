using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutScreen : MonoBehaviour
{
    [Header("Root panel to show/hide")]
    public GameObject panel;

    [Header("The god slots (one per available god)")]
    public GodSlot[] slots;

    [Header("Every god that exists today - no ownership system yet")]
    public List<GodDefinition> availableRoster;

    [Header("Confirm")]
    public Button startButton;

    [Header("Systems to unlock once the run begins")]
    public GodChorus chorus;
    public RoundDirector roundDirector;

    readonly HashSet<GodDefinition> selected = new HashSet<GodDefinition>();

    void Awake()
    {
        Time.timeScale = 0f;
        panel.SetActive(true);

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < availableRoster.Count)
            {
                var god = availableRoster[i];
                slots[i].gameObject.SetActive(true);
                selected.Add(god);   // default to fully-equipped, matching the "3 slots from the start" pillar
                slots[i].Setup(god, true, HandleToggle);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }

        if (startButton != null)
            startButton.onClick.AddListener(HandleStart);

        RefreshStartButton();
    }

    void HandleToggle(GodDefinition god, bool nowSelected)
    {
        if (nowSelected) selected.Add(god);
        else selected.Remove(god);
        RefreshStartButton();
    }

    void RefreshStartButton()
    {
        if (startButton != null) startButton.interactable = selected.Count > 0;
    }

    void HandleStart()
    {
        chorus.equippedGods = selected.ToList();

        panel.SetActive(false);
        Time.timeScale = 1f;

        chorus.enabled = true;
        roundDirector.enabled = true;
    }
}
