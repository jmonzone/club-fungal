using UnityEngine;

/// <summary>
/// Base class for modifying behavior priority based on game state.
/// Create as ScriptableObject assets and assign to UnitBehaviour components.
/// </summary>
public abstract class BehaviourPriorityModifier : ScriptableObject
{
    /// <summary>
    /// Returns a priority modifier value.
    /// - Return int.MinValue to block the behavior entirely
    /// - Return 0 for no change
    /// - Return positive values to boost priority
    /// - Return negative values to reduce priority
    /// </summary>
    public abstract int GetPriorityModifier();
}
