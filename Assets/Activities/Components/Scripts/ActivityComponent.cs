using UnityEngine;

public abstract class ActivityComponent : ScriptableObject
{
    protected NetworkRun networkRun;
    protected ActivityInstance activityInstance;


    public void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        this.networkRun = networkRun;
        this.activityInstance = activityInstance;
        OnInitialize();
    }

    protected virtual void OnInitialize()
    {
        // Optional initialization hook for components
    }

    public abstract void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance);
}
