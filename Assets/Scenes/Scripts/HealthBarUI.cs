using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;   // drag the player here
    public Image fillImage;             // the bar's fill (Image type = Filled)
    public TMP_Text label;                  // optional "8 / 10" text; or TMP_Text

    void Awake()
    {
        // was a 10px-tall muted-red sliver near the bottom of the screen - easy to
        // lose track of during gameplay. Bump both size and saturation without
        // touching the scene-authored position.
        if (fillImage != null)
        {
            fillImage.color = new Color(0.95f, 0.15f, 0.15f);
            var barRect = fillImage.transform.parent as RectTransform;
            if (barRect != null && barRect.sizeDelta.y < 18f)
                barRect.sizeDelta = new Vector2(barRect.sizeDelta.x, 18f);
        }
    }

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateBar;
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateBar;
    }

    void UpdateBar(int current, int max)
    {
        if (fillImage != null)
            fillImage.fillAmount = max > 0 ? (float)current / max : 0f;
        if (label != null)
            label.text = $"HP: {current} / {max}";
    }
}