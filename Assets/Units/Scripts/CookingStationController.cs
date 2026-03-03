using System.Collections;
using UnityEngine;

/// <summary>
/// Minimal controller for a static cooking station.
/// Proximity actions are handled by ProximityActionComponent.
/// Cooking logic is handled by CookingComponent.
/// </summary>
public class CookingStationController : UnitController
{
    [Header("Assignment")]
    [SerializeField] private Transform assignmentTransform;

    protected override IEnumerator Start()
    {
        // For static buildings without a UnitInstance, manually initialize components
        // Check if instance is actually initialized by checking if it has an ID
        if ((Instance == null || string.IsNullOrEmpty(Instance.Id)) && ComponentInstances.Count == 0)
        {
            InitializeComponents();
        }

        yield return base.Start();
    }

    /// <summary>
    /// Get the transform where assigned units should stand.
    /// </summary>
    public Transform GetAssignmentTransform()
    {
        return assignmentTransform != null ? assignmentTransform : transform;
    }
}
