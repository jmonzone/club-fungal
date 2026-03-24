using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Assigns the target Fungal to a stationary job at their current position.
/// Use this in place of StartActivityAction for stationary jobs — no activity UI is launched.
/// The Fungal will stop moving and stay at the assigned position until manually unassigned.
/// </summary>
[Serializable]
public class AssignToJobAction : InteractionAction
{
    public override void Execute(UnitController source, UnitController target, UnityAction onComplete)
    {
        var jobBehaviour = target.GetComponent<UnitJobBehaviour>();
        if (jobBehaviour == null)
        {
            Debug.LogWarning($"AssignToJobAction: {target.name} has no UnitJobBehaviour component.");
            onComplete?.Invoke();
            return;
        }

        jobBehaviour.Assign(target.transform.position);
        onComplete?.Invoke();
    }
}
