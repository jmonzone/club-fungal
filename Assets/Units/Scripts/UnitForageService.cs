using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Unit Forage Service")]
public class UnitForageService : GURUService
{
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private NetworkRunService networkRunService;

    private List<UnitForage> activeForagers = new List<UnitForage>();
    private Dictionary<IForageTarget, UnitForage> targetAssignments = new Dictionary<IForageTarget, UnitForage>();

    protected override void OnInitialize()
    {
        if (unitControllerService != null)
        {
            unitControllerService.OnUnitSummoned += OnUnitSummoned;
        }

        // Subscribe to unit re-entry events
        if (networkRunService != null)
        {
            networkRunService.OnUnitReenteredTether += ReassignSpores;
        }
    }

    private void OnDisable()
    {
        if (unitControllerService != null)
        {
            unitControllerService.OnUnitSummoned -= OnUnitSummoned;
        }

        activeForagers.Clear();
        targetAssignments.Clear();
    }


    private void OnUnitSummoned(UnitController controller)
    {
        var forager = controller.GetComponent<UnitForage>();
        if (forager != null && !activeForagers.Contains(forager))
        {
            activeForagers.Add(forager);
            forager.OnForageTargetReached += ReassignSpores;
            controller.OnBehaviourComplete += ReassignSpores;
        }
    }

    private void ReassignSpores()
    {
        if (networkRunService == null)
            return;

        Vector3 partyCenterGround = networkRunService.PartyCenterGround;
        float maxAssignmentDistance = networkRunService.MaxTetherDistance;

        // Refresh forager list to remove destroyed objects
        activeForagers.RemoveAll(f => f == null);

        // Collect all forage targets within range
        var forageTargets = new List<IForageTarget>();
        Collider[] colliders = Physics.OverlapSphere(partyCenterGround, maxAssignmentDistance);
        foreach (var collider in colliders)
        {
            var target = collider.GetComponentInParent<IForageTarget>();
            if (target != null && !forageTargets.Contains(target))
            {
                float targetDistanceFromCenter = Vector3.Distance(target.Transform.position, partyCenterGround);
                if (targetDistanceFromCenter <= maxAssignmentDistance)
                {
                    forageTargets.Add(target);
                }
            }
        }

        // Build a list of all valid (forager, target, distance) combinations
        var assignments = new List<(UnitForage forager, IForageTarget target, float distance)>();
        foreach (var forager in activeForagers)
        {
            foreach (var target in forageTargets)
            {
                float distance = Vector3.Distance(target.Transform.position, forager.transform.position);
                assignments.Add((forager, target, distance));
            }
        }

        // Sort by distance (closest first)
        assignments.Sort((a, b) => a.distance.CompareTo(b.distance));

        // Assign targets to foragers, prioritizing closest pairs
        var assignedForagers = new HashSet<UnitForage>();
        var assignedTargets = new HashSet<IForageTarget>();

        foreach (var assignment in assignments)
        {
            // Skip if either forager or target is already assigned
            if (assignedForagers.Contains(assignment.forager) || assignedTargets.Contains(assignment.target))
                continue;

            assignment.forager.SetTarget(assignment.target);
            assignedForagers.Add(assignment.forager);
            assignedTargets.Add(assignment.target);
        }

        // Clear targets for unassigned foragers
        foreach (var forager in activeForagers)
        {
            if (!assignedForagers.Contains(forager))
            {
                forager.SetTarget(null);
            }
        }
    }
}
