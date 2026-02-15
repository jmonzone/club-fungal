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
            ReassignSpores();
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

        // Track which targets have been assigned to avoid duplicates when possible
        var assignedTargets = new HashSet<IForageTarget>();

        var forageTargets = new List<IForageTarget>();
        Collider[] colliders = Physics.OverlapSphere(partyCenterGround, maxAssignmentDistance);
        foreach (var collider in colliders)
        {
            var target = collider.GetComponentInParent<IForageTarget>();
            if (target != null && !forageTargets.Contains(target))
            {
                forageTargets.Add(target);
            }
        }

        // For each forager, assign them to their closest available unassigned target
        foreach (var forager in activeForagers)
        {
            IForageTarget closestTarget = null;
            float closestDistance = float.MaxValue;

            // Find closest unassigned target (only one forager per target)
            foreach (var target in forageTargets)
            {
                if (assignedTargets.Contains(target)) continue;

                // Only consider targets within party tether range
                float targetDistanceFromCenter = Vector3.Distance(target.Transform.position, partyCenterGround);
                if (targetDistanceFromCenter > maxAssignmentDistance)
                    continue;

                float distance = Vector3.Distance(target.Transform.position, forager.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }

            // Assign the target (or null if none available) and mark it as assigned
            forager.SetTarget(closestTarget);
            if (closestTarget != null)
            {
                assignedTargets.Add(closestTarget);
            }
        }
    }
}
