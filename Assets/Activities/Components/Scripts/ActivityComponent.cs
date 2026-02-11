using UnityEngine;
using System.Collections.Generic;

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

    /// <summary>
    /// Called after all components are copied to allow remapping internal references
    /// </summary>
    public virtual void RemapReferences(Dictionary<ActivityComponent, ActivityComponent> originalToCopy)
    {
        // Override in components that hold references to other components
    }

    /// <summary>
    /// Determines if this component should update this frame
    /// </summary>
    public virtual bool ShouldUpdate()
    {
        return true;
    }

    /// <summary>
    /// Returns components that this component controls (and should not update independently)
    /// </summary>
    public virtual ActivityComponent[] GetControlledComponents()
    {
        return null;
    }

    public abstract void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance);
}
