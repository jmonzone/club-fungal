using UnityEngine;

public abstract class GURUService : ScriptableObject
{
    /// <summary>
    /// Called at app start, at reset, or when entering edit mode in the editor.
    /// </summary>
    public void Initialize()
    {
        OnInitialize();
    }

    /// <summary>
    /// Callded at app start, 
    /// </summary>
    protected abstract void OnInitialize();

    /// <summary>
    /// Called when a new scene has been loaded.
    /// </summary>
    public virtual void OnSceneLoaded() { }

    /// <summary>
    /// Called every frame by GameManager.
    /// </summary>
    public virtual void DoUpdate() { }
}