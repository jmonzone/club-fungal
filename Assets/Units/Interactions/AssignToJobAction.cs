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
    [SerializeField] private ActivityReference activityReference;

    public override void Execute(UnitController source, UnitController target, UnityAction onComplete)
    {
        var stationaryJob = target.GetComponent<UnitStationaryJob>();
        if (stationaryJob == null)
        {
            Debug.LogWarning($"AssignToJobAction: {target.name} has no UnitStationaryJob component.");
            onComplete?.Invoke();
            return;
        }

        stationaryJob.Assign(activityReference, target.transform.position);
        onComplete?.Invoke();
    }
}
