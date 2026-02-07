using UnityEngine;

public abstract class ActivityComponent : ScriptableObject
{
    public abstract void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance);
}
