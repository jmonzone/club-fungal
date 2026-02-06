using UnityEngine;

public abstract class ActivityComponent : ScriptableObject
{
    public abstract void Update(NetworkRun networkRun, ActivityInstance activityInstance);
}
