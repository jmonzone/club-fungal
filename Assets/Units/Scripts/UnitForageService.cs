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
    }

    public void RemoveTarget(IForageTarget target)
    {
        forageTargets.Remove(target);
        OnTargetsChanged?.Invoke();
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

        Debug.Log($"Reassigning targets to foragers... foragetargets: {forageTargets.Count}, activeForagers: {activeForagers.Count}");
        Vector3 partyCenterGround = networkRunService.PartyCenterGround;
        float maxAssignmentDistance = networkRunService.MaxTetherDistance;

        // Refresh forager list to remove destroyed objects
        activeForagers.RemoveAll(f => f == null);

        // Track which targets have been assigned to avoid duplicates when possible
        var assignedTargets = new HashSet<IForageTarget>();

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
