using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GodCooldownUI : MonoBehaviour
{
    public GodChorus chorus;
    public int godIndex = 0;
    public Image fillImage;   // Image type = Filled; fills up as the special becomes ready

    GodCompanion companion;
    TMP_Text label;

    void Update()
    {
        if (companion == null)
        {
            if (chorus == null || chorus.Active.Count <= godIndex) return;
            companion = chorus.Active[godIndex];
            EnsureLabel();
        }

        if (fillImage != null)
            fillImage.fillAmount = companion.SpecialCooldownNormalized;
    }

    // identifies which god this bar belongs to - there was no label at all before,
    // so 3 identical-looking colored bars in a row were meaningless at a glance
    void EnsureLabel()
    {
        if (label != null || fillImage == null) return;

        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(fillImage.transform.parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 2f);
        rt.sizeDelta = new Vector2(0f, 16f);

        label = go.AddComponent<TextMeshProUGUI>();
        label.text = companion.Definition.displayName;
        label.fontSize = 14;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
    }
}
