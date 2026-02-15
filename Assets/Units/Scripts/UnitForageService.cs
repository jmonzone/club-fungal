using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Services/Unit Forage Service")]
public class UnitForageService : GURUService
{
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private NetworkRunService networkRunService;

    private List<UnitForage> activeForagers = new List<UnitForage>();
    private Dictionary<IForageTarget, UnitForage> targetAssignments = new Dictionary<IForageTarget, UnitForage>();
    private List<IForageTarget> forageTargets = new List<IForageTarget>();

    public event UnityAction OnTargetsChanged;

    protected override void OnInitialize()
    {
        forageTargets = new List<IForageTarget>();

        if (unitControllerService != null)
        {
            unitControllerService.OnUnitSummoned += OnUnitSummoned;
        }
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        // Find all forage targets in scene
        var plantsInScene = FindObjectsOfType<PlantSporeEmitter>();
        foreach (var plant in plantsInScene)
        {
            RegisterTarget(plant);
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
        forageTargets.Clear();
    }

    public void RegisterTarget(IForageTarget target)
    {
        Debug.Log("Registering forage target: " + target.Transform.name);
        forageTargets.Add(target);
        OnTargetsChanged?.Invoke();
        ReassignSpores();
    }

    public void RemoveTarget(IForageTarget target)
    {
        forageTargets.Remove(target);
        OnTargetsChanged?.Invoke();
        ReassignSpores();
    }

    private void OnUnitSummoned(UnitController controller)
    {
        var forager = controller.GetComponent<UnitForage>();
        if (forager != null && !activeForagers.Contains(forager))
        {
            activeForagers.Add(forager);
            controller.OnBehaviourComplete += ReassignSpores;
            ReassignSpores();
        }
    }

    private void ReassignSpores()
    {
        if (networkRunService == null)
            return;

        Debug.Log($"Reassigning targets to foragers... foragetargets: {forageTargets.Count}, activeForagers: {activeForagers.Count}");
        Vector3 partyCenterGround = networkRunService.PartyCenterGround;
        float maxAssignmentDistance = networkRunService.MaxTetherDistance;

        // Refresh forager list to remove destroyed objects
        activeForagers.RemoveAll(f => f == null);

        // Clear old assignments
        targetAssignments.Clear();

        // Track which foragers have been assigned
        var assignedForagers = new HashSet<UnitForage>();

        // For each target within range, find the closest non-busy forager
        foreach (var target in forageTargets)
        {
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
            forager.SetTarget(assignedTarget);
        }
    }
}
