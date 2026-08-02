using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class XPBarUI : MonoBehaviour
{
    public XPManager xpManager;    // drag the XPManager here
    public Image fillImage;        // filled image for the XP bar
    public TMP_Text levelLabel;        // shows "Lv 3"; or TMP_Text

    void OnEnable()
    {
        if (xpManager != null)
        {
            xpManager.OnXPChanged += UpdateBar;
            xpManager.OnLevelUp += UpdateLevel;
        }
    }

    void OnDisable()
    {
        if (xpManager != null)
        {
            xpManager.OnXPChanged -= UpdateBar;
            xpManager.OnLevelUp -= UpdateLevel;
        }
    }

    void Start()
    {
        if (xpManager != null && levelLabel != null)
            levelLabel.text = "Lv " + xpManager.currentLevel;
    }

    void UpdateBar(int currentXP, int xpToNext)
    {
        if (fillImage != null)
            fillImage.fillAmount = xpToNext > 0 ? (float)currentXP / xpToNext : 0f;
    }

    void UpdateLevel(int newLevel)
    {
        if (levelLabel != null)
            levelLabel.text = "Lv " + newLevel;
    }
}