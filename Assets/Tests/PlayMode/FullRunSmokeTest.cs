using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Stopwatch = System.Diagnostics.Stopwatch;

// Drives an entire run (loadout confirm -> round 1 -> miniboss @10 -> boss @20)
// at accelerated Time.timeScale and asserts it reaches a terminal state (win or
// player death) with no exceptions/errors logged. This does NOT judge balance/fun
// - only that the wiring (LoadoutScreen enabling GodChorus/RoundDirector, boss
// phase transitions, DamageBus routing, round progression) doesn't break at runtime.
public class FullRunSmokeTest
{
    const string SceneName = "SampleScene";
    const float TimeScale = 8f;
    const float RealTimeoutSeconds = 240f;

    [UnityTest]
    [Timeout(600000)] // NUnit safety net above RealTimeoutSeconds; the loop below should exit first
    public IEnumerator FullRun_ReachesTerminalState_WithNoErrors()
    {
        // Time.maximumDeltaTime caps sim-time gained per frame (default 0.333s) - in this
        // headless/CPU-bound run real frames are slower than that cap, so TimeScale alone
        // does nothing until this ceiling is raised too.
        Time.maximumDeltaTime = 2f;

        // must happen before the scene loads: MasteryScreen reads coins once when it
        // builds/refreshes itself in LoadoutScreen.Awake() and won't re-check until
        // something calls Refresh() again, so granting coins after that point leaves
        // the upgrade button's `interactable` stuck false - a real click correctly
        // does nothing to a non-interactable Button (this was a test-ordering bug,
        // not a game bug, but it's exactly the kind of thing worth getting right)
        SaveManager.AddCoins(1000);

        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
        yield return null;
        yield return null; // let Awake/Start settle

        Assert.IsNotNull(EventSystem.current, "No EventSystem in scene - can't simulate real UI clicks");

        var loadout = Object.FindFirstObjectByType<LoadoutScreen>();
        var roundDirector = Object.FindFirstObjectByType<RoundDirector>();
        var playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        var cardScreen = Object.FindFirstObjectByType<CardScreen>();

        Assert.IsNotNull(loadout, "LoadoutScreen not found in scene");
        Assert.IsNotNull(roundDirector, "RoundDirector not found in scene");
        Assert.IsNotNull(playerHealth, "PlayerHealth not found in scene");
        Assert.IsNotNull(cardScreen, "CardScreen not found in scene");
        Assert.IsNotNull(loadout.chorus, "LoadoutScreen.chorus not wired");
        Assert.IsNotNull(loadout.startButton, "LoadoutScreen.startButton not wired");
        Assert.IsNotNull(loadout.panel, "LoadoutScreen.panel not wired");

        Assert.IsFalse(roundDirector.enabled, "RoundDirector should start disabled until loadout is confirmed");
        Assert.IsFalse(loadout.chorus.enabled, "GodChorus should start disabled until loadout is confirmed");

        // regression: the loadout panel defaults to active in the scene and must be
        // explicitly hidden while the mastery screen is up, or their text overlaps on screen
        Assert.IsFalse(loadout.panel.activeSelf, "LoadoutScreen.panel should stay hidden behind the mastery screen until Continue is clicked");

        // --- Mastery screen (shown first, ahead of the loadout panel) ---
        const string testGodId = "eclipsed_eye";
        int levelBefore = SaveManager.GetMastery(testGodId);
        int coinsBeforeUpgrade = SaveManager.Current.coins;
        int cost = SaveManager.GetMasteryCost(testGodId);

        var upgradeButton = GameObject.Find("Button_" + testGodId)?.GetComponent<Button>();
        Assert.IsNotNull(upgradeButton, "MasteryScreen upgrade button not found for " + testGodId);
        SimulateClick(upgradeButton.gameObject);

        Assert.AreEqual(levelBefore + 1, SaveManager.GetMastery(testGodId), "Mastery level did not increase after clicking upgrade");
        Assert.AreEqual(coinsBeforeUpgrade - cost, SaveManager.Current.coins, "Coins were not deducted correctly by the mastery upgrade");

        var continueButton = GameObject.Find("ContinueButton")?.GetComponent<Button>();
        Assert.IsNotNull(continueButton, "MasteryScreen continue button not found");
        SimulateClick(continueButton.gameObject);
        yield return null;

        Assert.IsTrue(loadout.panel.activeSelf, "LoadoutScreen.panel should be shown after the mastery screen's Continue");

        SimulateClick(loadout.startButton.gameObject);
        yield return null;

        Assert.IsTrue(roundDirector.enabled, "RoundDirector was not enabled by LoadoutScreen.HandleStart");
        Assert.IsTrue(loadout.chorus.enabled, "GodChorus was not enabled by LoadoutScreen.HandleStart");
        Assert.AreEqual(1f, Time.timeScale, 0.001f, "Time.timeScale was not restored after confirming loadout");

        bool victory = false;
        bool died = false;
        int lastRound = 0;

        roundDirector.OnVictory += () => victory = true;
        playerHealth.OnDied += () => died = true;
        roundDirector.OnRoundChanged += r =>
        {
            lastRound = r;
            Debug.Log($"[SmokeTest] Round {r} started");
        };

        Time.timeScale = TimeScale;
        var sw = Stopwatch.StartNew();

        int cardsPicked = 0;
        while (!victory && !died && sw.Elapsed.TotalSeconds < RealTimeoutSeconds)
        {
            // WaveManager.ShowUpgradeChoices() sets Time.timeScale = 0 and waits for a
            // click on a CardSlot's button; nothing does that on its own in a headless run.
            if (cardScreen.panel != null && cardScreen.panel.activeSelf)
            {
                // let Unity's normal per-frame layout pass settle the card row's
                // LayoutGroup-driven positions before reading them - LayoutRebuilder.
                // ForceRebuildLayoutImmediate on the canvas root wasn't sufficient here.
                // Time.timeScale = 0 (WaveManager pauses while this is up) doesn't stop
                // frames/Update from happening, only deltaTime scaling.
                yield return null;
                yield return null;

                foreach (var slot in cardScreen.slots)
                {
                    if (slot != null && slot.gameObject.activeSelf && slot.button != null)
                    {
                        SimulateClick(slot.button.gameObject);
                        cardsPicked++;
                        break;
                    }
                }
            }
            yield return null;
        }

        Time.timeScale = 1f;

        Debug.Log($"[SmokeTest] Finished - victory={victory} died={died} lastRound={lastRound} cardsPicked={cardsPicked} realSeconds={sw.Elapsed.TotalSeconds:F1}");

        Assert.IsTrue(victory || died,
            $"Run never reached a terminal state within {RealTimeoutSeconds}s real time (stuck at round {lastRound}) - looks like a hang, not a balance issue.");
    }

    // Simulates an actual mouse click: raycasts through every GraphicRaycaster in the
    // scene at the button's own screen position - exactly what a real click does - and
    // asserts the button itself is what gets hit. Calling .onClick.Invoke() directly
    // can't catch a button being covered or mispositioned (an inactive-panel overlap and
    // a zero-size-rect text overlap both slipped through that way before this existed).
    static void SimulateClick(GameObject buttonGO)
    {
        var rt = buttonGO.GetComponent<RectTransform>();
        Assert.IsNotNull(rt, buttonGO.name + " has no RectTransform");

        var canvas = rt.GetComponentInParent<Canvas>();

        // a panel that just became active this same frame (e.g. a card screen shown
        // mid-level-up) may sit under a LayoutGroup whose children still carry
        // placeholder zero anchors/size until a layout pass resolves them - a real
        // player's click always lands well after that settles naturally, so force it
        // here rather than flaking on 1-frame timing. Canvas.ForceUpdateCanvases()
        // alone isn't enough - it only flushes rebuilds already queued, not ones that
        // haven't been marked dirty yet.
        if (canvas != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(canvas.GetComponent<RectTransform>());

        var cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, rt.position);

        var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count == 0)
        {
            Debug.Log($"[SimulateClick DIAG] target={buttonGO.name} worldPos={rt.position} screenPos={screenPos} " +
                $"Screen=({Screen.width}x{Screen.height}) canvas={(canvas != null ? canvas.name : "null")} " +
                $"renderMode={(canvas != null ? canvas.renderMode.ToString() : "n/a")} " +
                $"worldCamera={(canvas != null && canvas.worldCamera != null ? canvas.worldCamera.name : "null")} " +
                $"pixelRect={(canvas != null ? canvas.pixelRect.ToString() : "n/a")} " +
                $"anchoredPos={rt.anchoredPosition} sizeDelta={rt.sizeDelta} lossyScale={rt.lossyScale} " +
                $"activeInHierarchy={buttonGO.activeInHierarchy}");
        }

        Assert.IsTrue(results.Count > 0, $"Click at {buttonGO.name}'s screen position hit nothing");
        var hit = results[0].gameObject;
        Assert.IsTrue(hit == buttonGO || hit.transform.IsChildOf(buttonGO.transform) || buttonGO.transform.IsChildOf(hit.transform),
            $"Click at {buttonGO.name}'s screen position hit '{hit.name}' instead - something is covering or mispositioning it");

        ExecuteEvents.ExecuteHierarchy(hit, pointerData, ExecuteEvents.pointerClickHandler);
    }
}
