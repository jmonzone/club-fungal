using UnityEngine;

public abstract class GURUService : ScriptableObject
{
    /// <summary>
    /// Initializes the service.
    /// </summary>
    internal void Initialize()
    {
        OnInitialize();
    }

    protected abstract void OnInitialize();

    /// <summary>
    /// Called when a new scene has been loaded.
    /// </summary>
    public virtual void OnSceneLoaded() { }
}