using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Builds its own Canvas at runtime instead of being scene-authored, so it can
// exist without touching SampleScene.unity - styling is a solid-color placeholder,
// swap for real layout/art whenever it's worth a designer's time (same tier as
// the numbered card icons and TempLineFlash placeholders elsewhere in the project).
public class MasteryScreen : MonoBehaviour
{
    class Row
    {
        public GodDefinition god;
        public TMP_Text label;
        public Button button;
        public TMP_Text buttonLabel;
    }

    GameObject panel;
    TMP_Text coinsLabel;
    readonly List<Row> rows = new List<Row>();
    Action onContinue;

    public void ShowThenContinue(List<GodDefinition> roster, Action continueCallback)
    {
        onContinue = continueCallback;
        if (panel == null) Build(roster);
        panel.SetActive(true);
        Refresh();
    }

    void Build(List<GodDefinition> roster)
    {
        // deliberately NOT parented under this component's own transform: this script
        // gets added to the same GameObject as the scene's main "UIManager" Canvas, and
        // a Canvas nested under another Canvas gets its render-mode/full-screen sizing
        // overridden by the parent instead of behaving as an independent overlay - it
        // rendered as a tiny square sized to a default RectTransform instead of the
        // whole screen. Keeping this as an unparented scene root avoids that entirely.
        var canvasGO = new GameObject("MasteryCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(canvasGO.transform, false);
        StretchFull(panel.GetComponent<RectTransform>());
        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

        // NOTE: anchorMin must differ from anchorMax - equal anchors collapse the rect to
        // zero size, and TMP's default Overflow mode then renders the text sprawled out
        // from that single point with no bounds (this broke Title/Coins once already).
        MakeText(panel.transform, "Title", "Mastery", 32, new Vector2(0.3f, 0.88f), new Vector2(0.7f, 0.96f));
        coinsLabel = MakeText(panel.transform, "Coins", "", 22, new Vector2(0.3f, 0.8f), new Vector2(0.7f, 0.86f));

        float y = 0.65f;
        foreach (var god in roster)
        {
            var label = MakeText(panel.transform, "Label_" + god.id, "", 20,
                new Vector2(0.2f, y), new Vector2(0.58f, y + 0.08f), TextAlignmentOptions.MidlineLeft);

            var (button, buttonLabel) = MakeButton(panel.transform, "Button_" + god.id,
                new Vector2(0.62f, y), new Vector2(0.82f, y + 0.08f));

            var row = new Row { god = god, label = label, button = button, buttonLabel = buttonLabel };
            rows.Add(row);

            var capturedGod = god;
            button.onClick.AddListener(() => HandleUpgrade(capturedGod));

            y -= 0.12f;
        }

        var (continueButton, continueLabel) = MakeButton(panel.transform, "ContinueButton",
            new Vector2(0.4f, 0.08f), new Vector2(0.6f, 0.16f));
        continueLabel.text = "Continue";
        continueButton.onClick.AddListener(HandleContinue);
    }

    void HandleUpgrade(GodDefinition god)
    {
        SaveManager.TryUpgradeMastery(god.id);   // no-op if maxed or can't afford - Refresh reflects either way
        Refresh();
    }

    void HandleContinue()
    {
        panel.SetActive(false);
        onContinue?.Invoke();
    }

    void Refresh()
    {
        coinsLabel.text = $"Coins: {SaveManager.Current.coins}";

        foreach (var row in rows)
        {
            int level = SaveManager.GetMastery(row.god.id);
            int cost = SaveManager.GetMasteryCost(row.god.id);
            row.label.text = $"{row.god.displayName}  (Lv. {level}/{SaveManager.MaxMasteryLevel})";

            if (cost < 0)
            {
                row.buttonLabel.text = "MAX";
                row.button.interactable = false;
            }
            else
            {
                row.buttonLabel.text = $"Upgrade ({cost}c)";
                row.button.interactable = SaveManager.Current.coins >= cost;
            }
        }
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static TMP_Text MakeText(Transform parent, string name, string text, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;   // wraps to one character per line otherwise - every label here is meant to be one line
        tmp.enableAutoSizing = true;                       // shrink to fit its box rather than overflow past it at a fixed size
        tmp.fontSizeMin = 8;
        tmp.fontSizeMax = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.raycastTarget = false;   // plain text should never be able to block a click meant for a button
        return tmp;
    }

    static (Button, TMP_Text) MakeButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        go.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.28f, 1f);
        var button = go.AddComponent<Button>();

        var label = MakeText(go.transform, "Text", "", 18, Vector2.zero, Vector2.one);
        return (button, label);
    }
}
