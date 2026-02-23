using UnityEngine;

/// <summary>
/// Base class for proximity actions that can be triggered when player interacts with a unit.
/// </summary>
public abstract class ProximityAction : ScriptableObject
{
    /// <summary>
    /// Called when the proximity action is triggered.
    /// </summary>
    /// <param name="controller">The unit controller this action is attached to</param>
    public abstract void Execute(UnitController controller);
}
