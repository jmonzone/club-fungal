using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Unit Forage Service")]
public class UnitForageService : GURUService
{
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private NetworkRunService networkRunService;

    private List<UnitForage> activeForagers = new List<UnitForage>();
    private List<MonoBehaviour> forageables = new List<MonoBehaviour>();
    private Dictionary<IForageTarget, UnitForage> targetAssignments = new Dictionary<IForageTarget, UnitForage>();

    protected override void OnInitialize()
    {
        forageables = new List<MonoBehaviour>();

        if (unitControllerService != null)
        {
            unitControllerService.OnUnitSummoned += OnUnitSummoned;
        }
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();
        DiscoverForageables();
    }

    private void DiscoverForageables()
    {
        forageables.Clear();

        // Find all IForageTarget components in the scene
        // Don't filter by distance here - let ReassignTargets() handle dynamic filtering as party moves
        var allForageables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var component in allForageables)
        {
            if (component is IForageTarget)
            {
                forageables.Add(component);
            }
        }

        ReassignTargets();
    }

    private void OnDisable()
    {
        if (unitControllerService != null)
        {
            unitControllerService.OnUnitSummoned -= OnUnitSummoned;
        }

        activeForagers.Clear();
        forageables.Clear();
        targetAssignments.Clear();
    }

    private List<IForageTarget> GetAllForageables()
    {
        var result = new List<IForageTarget>();
        forageables.RemoveAll(f => f == null); // Clean up destroyed objects

        foreach (var forageable in forageables)
        {
            if (forageable is IForageTarget target)
            {
                result.Add(target);
            }
        }

        return result;
    }

    private void OnUnitSummoned(UnitController controller)
    {
        var forager = controller.GetComponent<UnitForage>();
        if (forager != null && !activeForagers.Contains(forager))
        {
            activeForagers.Add(forager);
            controller.OnBehaviourCompleted += () => OnUnitBehaviourChanged(controller);
            ReassignTargets();
        }
    }

    private void OnUnitBehaviourChanged(UnitController controller)
    {
        ReassignTargets();
    }

    private void ReassignTargets()
    {
        if (networkRunService == null)
            return;

        Vector3 partyCenterGround = networkRunService.PartyCenterGround;
        float maxAssignmentDistance = networkRunService.MaxTetherDistance;

        // Refresh forager list to remove destroyed objects
        activeForagers.RemoveAll(f => f == null);

        // Clear old assignments
        targetAssignments.Clear();

        // Get all forageable targets
        var allTargets = GetAllForageables();

        // Track which foragers have been assigned
        var assignedForagers = new HashSet<UnitForage>();

        // For each target within range, find the closest non-busy forager
        foreach (var target in allTargets)
        {
            if (!target.IsAvailable) continue;

            // Only assign targets within party tether range
            float targetDistanceFromCenter = Vector3.Distance(target.Transform.position, partyCenterGround);
            if (targetDistanceFromCenter > maxAssignmentDistance)
                continue;

            UnitForage closestForager = null;
            float closestDistance = float.MaxValue;

            foreach (var forager in activeForagers)
            {
                // Skip foragers that already have an assignment
                if (assignedForagers.Contains(forager)) continue;

                float distance = Vector3.Distance(target.Transform.position, forager.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestForager = forager;
                }
            }

            // Assign target to the closest available forager
            if (closestForager != null)
            {
                targetAssignments[target] = closestForager;
                assignedForagers.Add(closestForager);
            }
        }

        // Update all foragers with their assignments
        foreach (var forager in activeForagers)
        {
            var assignedTarget = targetAssignments.FirstOrDefault(kvp => kvp.Value == forager).Key;
            if (assignedTarget != null)
            {
                forager.SetTarget(assignedTarget);
            }
        }
    }
}
