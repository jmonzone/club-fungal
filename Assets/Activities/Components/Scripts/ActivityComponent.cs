using UnityEngine;

public abstract class ActivityComponent : ScriptableObject
{
    public virtual void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Optional initialization hook for components
    }

    public abstract void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance);
}
