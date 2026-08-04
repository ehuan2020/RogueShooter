using UnityEngine;

// base class — each special type is a subclass with its own Activate
public abstract class GodSpecial : ScriptableObject
{
    public abstract void Activate(GodCompanion god, Transform player);
}