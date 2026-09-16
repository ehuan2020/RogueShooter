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

    [Header("Every god that exists today - filtered to what's owned at runtime")]
    public List<GodDefinition> availableRoster;

    [Header("Confirm")]
    public Button startButton;

    [Header("Systems to unlock once the run begins")]
    public GodChorus chorus;
    public RoundDirector roundDirector;

    readonly HashSet<GodDefinition> selected = new HashSet<GodDefinition>();
    List<GodDefinition> owned;

    void Awake()
    {
        Time.timeScale = 0f;
        panel.SetActive(false);   // stays hidden until the mastery screen's Continue - it defaults to active in the scene

        var save = SaveManager.Current;
        owned = availableRoster.Where(g => save.ownedGodIds.Contains(g.id)).ToList();

        var mastery = gameObject.AddComponent<MasteryScreen>();
        mastery.ShowThenContinue(owned, ShowLoadoutPanel);
    }

    void ShowLoadoutPanel()
    {
        panel.SetActive(true);

        var save = SaveManager.Current;

        // resume the last saved loadout if there is one; otherwise default to
        // fully-equipped, matching the "3 slots from the start" pillar
        bool hasSavedLoadout = save.equippedGodIds.Count > 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < owned.Count)
            {
                var god = owned[i];
                bool startSelected = hasSavedLoadout ? save.equippedGodIds.Contains(god.id) : true;
                slots[i].gameObject.SetActive(true);
                if (startSelected) selected.Add(god);
                slots[i].Setup(god, startSelected, HandleToggle);
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

        SaveManager.Current.equippedGodIds = selected.Select(g => g.id).ToList();
        SaveManager.Save();

        panel.SetActive(false);
        Time.timeScale = 1f;

        chorus.enabled = true;
        roundDirector.enabled = true;
    }
}
