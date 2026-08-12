using UnityEngine;
using UnityEngine.UI;

public class GodCooldownUI : MonoBehaviour
{
    public GodChorus chorus;
    public int godIndex = 0;
    public Image fillImage;   // Image type = Filled; fills up as the special becomes ready

    GodCompanion companion;

    void Update()
    {
        if (companion == null)
        {
            if (chorus == null || chorus.Active.Count <= godIndex) return;
            companion = chorus.Active[godIndex];
        }

        if (fillImage != null)
            fillImage.fillAmount = companion.SpecialCooldownNormalized;
    }
}
