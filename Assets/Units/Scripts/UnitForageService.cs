using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Unit Forage Service")]
public class UnitForageService : GURUService
{
    [SerializeField] private SporeReference sporeReference;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private NetworkRunService networkRunService;

    private List<UnitForage> activeForagers = new List<UnitForage>();
    private Dictionary<SporeController, UnitForage> sporeAssignments = new Dictionary<SporeController, UnitForage>();

    protected override void OnInitialize()
    {
        if (sporeReference != null)
        {
            sporeReference.OnSporeControllersChanged += ReassignSpores;
        }
        if (unitControllerService != null)
        {
            unitControllerService.OnUnitSummoned += OnUnitSummoned;
        }
    }

    private void OnDisable()
    {
        if (sporeReference != null)
        {
            sporeReference.OnSporeControllersChanged -= ReassignSpores;
        }
        if (unitControllerService != null)
        {
            unitControllerService.OnUnitSummoned -= OnUnitSummoned;
        }

        activeForagers.Clear();
        sporeAssignments.Clear();
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
        if (sporeReference == null || networkRunService == null)
            return;

        Debug.Log("Reassigning spores to foragers...");
        Vector3 partyCenterGround = networkRunService.PartyCenterGround;
        float maxAssignmentDistance = networkRunService.MaxTetherDistance;

        // Refresh forager list to remove destroyed objects
        activeForagers.RemoveAll(f => f == null);

        // Clear old assignments
        sporeAssignments.Clear();

        // Track which foragers have been assigned
        var assignedForagers = new HashSet<UnitForage>();

        // For each spore within range, find the closest non-busy forager
        foreach (var spore in sporeReference.SporeControllers)
        {
            // Only assign spores within party tether range
            float sporeDistanceFromCenter = Vector3.Distance(spore.transform.position, partyCenterGround);
            if (sporeDistanceFromCenter > maxAssignmentDistance)
                continue;

            UnitForage closestForager = null;
            float closestDistance = float.MaxValue;

            foreach (var forager in activeForagers)
            {
                // Skip foragers that already have an assignment
                if (assignedForagers.Contains(forager)) continue;

                float distance = Vector3.Distance(spore.transform.position, forager.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestForager = forager;
                }
            }

            // Assign spore to the closest available forager
            if (closestForager != null)
            {
                sporeAssignments[spore] = closestForager;
                assignedForagers.Add(closestForager);
            }
        }

        // Update all foragers with their assignments
        foreach (var forager in activeForagers)
        {
            var assignedSpore = sporeAssignments.FirstOrDefault(kvp => kvp.Value == forager).Key;
            forager.SetTargetSpore(assignedSpore);
        }
    }
}
