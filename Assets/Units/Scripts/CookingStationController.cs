using System.Collections;
using UnityEngine;

/// <summary>
/// Minimal controller for a static cooking station.
/// Proximity actions are handled by ProximityActionComponent.
/// Cooking logic is handled by CookingComponent.
/// </summary>
public class CookingStationController : UnitController
{
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
}
