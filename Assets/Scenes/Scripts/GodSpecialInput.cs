using UnityEngine;
using UnityEngine.InputSystem;

public class GodSpecialInput : MonoBehaviour
{
    public GodChorus chorus;
    public Key[] triggerKeys = { Key.Q, Key.E, Key.R };   // temporary test bindings, one per equipped god slot

    void Update()
    {
        if (chorus == null || Keyboard.current == null) return;

        for (int i = 0; i < triggerKeys.Length; i++)
        {
            if (Keyboard.current[triggerKeys[i]].wasPressedThisFrame)
                chorus.TriggerSpecial(i);
        }
    }
}
