using UnityEngine;

public abstract class GURUService : ScriptableObject
{
    internal void Initialize()
    {
        OnInitialize();
    }

    protected abstract void OnInitialize();
}