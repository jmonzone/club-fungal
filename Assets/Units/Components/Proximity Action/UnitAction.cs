using UnityEngine;

/// <summary>
/// Base class for actions that can be triggered on units.
/// Used by proximity actions, interactions, or other unit-based triggers.
/// </summary>
public abstract class UnitAction : ScriptableObject
{
    /// <summary>
    /// Called when the action is triggered.
    /// </summary>
    /// <param name="targetUnit">The unit this action is being performed on (e.g., building/station)</param>
    public abstract void Execute(UnitController targetUnit);
}
