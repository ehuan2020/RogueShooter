using UnityEngine;
using TMPro;

public class RoundCounterUI : MonoBehaviour
{
    public RoundDirector roundDirector;   // drag the RoundDirector here
    public TMP_Text label;                // shows "Round 3 / 20"

    void OnEnable()
    {
        if (roundDirector != null)
        {
            roundDirector.OnRoundChanged += UpdateLabel;
            roundDirector.OnVictory += HandleVictory;
        }
    }

    void OnDisable()
    {
        if (roundDirector != null)
        {
            roundDirector.OnRoundChanged -= UpdateLabel;
            roundDirector.OnVictory -= HandleVictory;
        }
    }

    void UpdateLabel(int round)
    {
        if (label != null)
            label.text = $"Round {round} / {roundDirector.totalRounds}";
    }

    void HandleVictory()
    {
        if (label != null)
            label.text = "Victory!";
    }
}
