using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Unit Forage Service")]
public class UnitForageService : GURUService
{
    [SerializeField] private SporeReference sporeReference;
    [SerializeField] private UnitControllerService unitControllerService;

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
            ReassignSpores();
        }
    }

    private void ReassignSpores()
    {
        if (sporeReference == null) return;

        // Refresh forager list to remove destroyed objects
        activeForagers.RemoveAll(f => f == null);

        // Clear old assignments
        sporeAssignments.Clear();

        // Get available spores
        var availableSpores = new List<SporeController>(sporeReference.SporeControllers);

        // Assign spores to foragers based on proximity
        foreach (var forager in activeForagers)
        {
            SporeController closestSpore = null;
            float closestDistance = float.MaxValue;

            foreach (var spore in availableSpores)
            {
                float distance = Vector3.Distance(forager.transform.position, spore.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSpore = spore;
                }
            }

            if (closestSpore != null)
            {
                sporeAssignments[closestSpore] = forager;
                availableSpores.Remove(closestSpore);
                forager.SetTargetSpore(closestSpore);
            }
            else
            {
                forager.SetTargetSpore(null);
            }
        }
    }
}
