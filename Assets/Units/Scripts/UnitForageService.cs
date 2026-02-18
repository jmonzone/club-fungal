using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Unit Forage Service")]
public class UnitForageService : GURUService
{
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private NetworkRunService networkRunService;

    [Header("Camera Movement Settings")]
    [SerializeField] private float maxCameraVelocityForForaging = 2f;

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

    public override void DoUpdate()
    {
        // Continuously check if any foragers have targets that are now out of range
        CheckAndClearOutOfRangeTargets();
    }

    private void CheckAndClearOutOfRangeTargets()
    {
        if (networkRunService == null)
            return;

        Vector3 partyCenterGround = networkRunService.PartyCenterGround;
        float maxAssignmentDistance = networkRunService.MaxTetherDistance;

        // Clear targets for foragers whose current targets are now outside tether range
        foreach (var forager in activeForagers)
        {
            if (forager != null && forager.CurrentTarget != null)
            {
                float targetDistanceFromCenter = Vector3.Distance(forager.CurrentTarget.Transform.position, partyCenterGround);
                if (targetDistanceFromCenter > maxAssignmentDistance)
                {
                    // Target is out of range, clear it
                    forager.SetTarget(null);
                }
            }
        }
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

        // Don't assign new forage targets if camera is moving too fast
        float cameraSpeed = networkRunService.CameraVelocity.magnitude;
        if (cameraSpeed > maxCameraVelocityForForaging)
        {
            // Clear targets for all foragers when camera is moving fast
            foreach (var forager in activeForagers)
            {
                if (forager != null)
                {
                    forager.SetTarget(null);
                }
            }
            return;
        }

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
        // Multiple foragers can share the same target if there are more foragers than targets
        var assignedForagers = new HashSet<UnitForage>();
        var targetToForagers = new Dictionary<IForageTarget, List<UnitForage>>();

        foreach (var assignment in assignments)
        {
            // Skip if forager is already assigned
            if (assignedForagers.Contains(assignment.forager))
                continue;

            if (!targetToForagers.ContainsKey(assignment.target))
            {
                targetToForagers[assignment.target] = new List<UnitForage>();
            }
            targetToForagers[assignment.target].Add(assignment.forager);
            assignedForagers.Add(assignment.forager);
        }

        // Assign positions to foragers, distributing them evenly around each target
        foreach (var kvp in targetToForagers)
        {
            IForageTarget target = kvp.Key;
            List<UnitForage> foragers = kvp.Value;

            if (foragers.Count == 1)
            {
                // Single forager - use target's position
                foragers[0].SetTarget(target, target.Transform.position);
            }
            else
            {
                // Multiple foragers - distribute them evenly around the target
                float angleStep = 360f / foragers.Count;
                float forageRadius = 1.5f; // Distance from target center

                for (int i = 0; i < foragers.Count; i++)
                {
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(angle) * forageRadius, 0, Mathf.Sin(angle) * forageRadius);
                    Vector3 assignedPosition = target.Transform.position + offset;
                    foragers[i].SetTarget(target, assignedPosition);
                }
            }
        }

        // Clear targets for unassigned foragers OR foragers whose targets are no longer in range
        foreach (var forager in activeForagers)
        {
            if (!assignedForagers.Contains(forager))
            {
                forager.SetTarget(null);
            }
        }
    }
}
