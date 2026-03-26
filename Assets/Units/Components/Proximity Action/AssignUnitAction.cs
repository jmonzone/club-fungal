using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Action that requests unit assignment UI to be shown.
/// Fires events that UI components subscribe to.
/// </summary>
[CreateAssetMenu(fileName = "AssignUnitAction", menuName = "Club Fungal/Unit Actions/Assign Unit")]
public class AssignUnitAction : UnitAction
{
    [Header("Services")]
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private ControlModeService controlModeService;

    public UnitControllerService UnitControllerService => unitControllerService;
    public UnitInstanceService UnitInstanceService => unitInstanceService;

    // Store the building controller that triggered this action
    private UnitController currentBuildingController;

    public override void Execute(UnitController buildingController)
    {
        if (buildingController == null)
        {
            return;
        }

        currentBuildingController = buildingController;

        // Request UI to be shown via service
        if (controlModeService != null)
        {
            controlModeService.ShowAssignUnitUI(buildingController, this);
        }
    }

    /// <summary>
    /// Called by UI when a unit is selected.
    /// Assigns the unit to the building.
    /// </summary>
    public void SelectUnit(UnitController workerUnit)
    {
        if (workerUnit == null || currentBuildingController == null)
        {
            return;
        }

        AssignUnit(workerUnit, currentBuildingController);

        // Hide assign UI
        if (controlModeService != null)
        {
            controlModeService.HideAssignUnitUI();
        }
    }

    /// <summary>
    /// Assigns a worker unit to a building.
    /// Teleports the unit, applies components, and saves state.
    /// </summary>
    private void AssignUnit(UnitController workerUnit, UnitController buildingController)
    {
        if (workerUnit == null || buildingController == null)
        {
            Debug.LogWarning("AssignUnitAction: Cannot assign - null controller");
            return;
        }

        Debug.Log($"Assigning {workerUnit.name} to {buildingController.name}");

        // Get assignable station component from building
        var station = buildingController.GetComponent<AssignableStation>();
        if (station == null)
        {
            Debug.LogWarning($"AssignUnitAction: Building {buildingController.name} missing AssignableStation component");
            return;
        }

        // Move unit to assignment position but keep current parent (don't parent under building)
        workerUnit.Teleport(station.AssignmentTransform.position, workerUnit.transform.parent);

        // Apply component to worker (e.g., cooking component)
        if (station.WorkerComponent != null)
        {
            var componentInstance = station.WorkerComponent.CreateInstance(workerUnit);
            workerUnit.ComponentInstances.Add(componentInstance);
            componentInstance.OnInitialize();
        }

        // Swap proximity action component on the building
        // Remove unassigned proximity action (assignment UI)
        if (station.UnassignedProximityAction != null)
        {
            var unassignedComponent = buildingController.GetComponentInstance<ProximityActionComponentInstance>();
            if (unassignedComponent != null && unassignedComponent.Definition == station.UnassignedProximityAction)
            {
                buildingController.RemoveComponent(unassignedComponent);
            }
        }

        // Add assigned proximity action (worker info/progress UI)
        if (station.AssignedProximityAction != null)
        {
            var assignedComponent = station.AssignedProximityAction.CreateInstance(buildingController);
            buildingController.ComponentInstances.Add(assignedComponent);
            assignedComponent.OnInitialize();
        }

        // Update station with assigned unit reference
        station.SetAssignedUnit(workerUnit);

        // If this unit was in manual control mode, transition to UnitSelected mode
        // (keep them selected but disable manual control while assigned)
        if (controlModeService != null && 
            controlModeService.IsManualControlActive() && 
            controlModeService.SelectedUnit == workerUnit.Instance)
        {
            controlModeService.SelectUnit(workerUnit.Instance);
        }

        // Save state
        if (unitInstanceService != null)
        {
            unitInstanceService.SaveData();
        }
    }

    /// <summary>
    /// Unassigns a worker unit from their assigned building.
    /// Removes work components and clears assignment state.
    /// </summary>
    public void UnassignUnit(UnitController workerUnit)
    {
        if (workerUnit == null)
        {
            Debug.LogWarning("AssignUnitAction: Cannot unassign - null worker unit");
            return;
        }

        // Find the building this unit was assigned to via AssignableStation
        UnitController assignedBuilding = null;
        AssignableStation station = null;
        if (unitControllerService != null)
        {
            foreach (var controller in unitControllerService.Controllers)
            {
                var stationComponent = controller.GetComponent<AssignableStation>();
                if (stationComponent != null && stationComponent.AssignedUnit == workerUnit)
                {
                    assignedBuilding = controller;
                    station = stationComponent;
                    break;
                }
            }
        }

        // Remove the work component from worker
        if (assignedBuilding != null && station != null)
        {
            if (station.WorkerComponent != null)
            {
                // Find and remove the component instance that matches the station's worker component type
                var componentToRemove = workerUnit.ComponentInstances
                    .FirstOrDefault(c => c.Definition == station.WorkerComponent);
                if (componentToRemove != null)
                {
                    // Cancel any ongoing work before removing the component
                    if (componentToRemove is CookingComponentInstance cookingComponent)
                    {
                        cookingComponent.CancelCooking();
                    }

                    workerUnit.RemoveComponent(componentToRemove);
                }
            }

            // Swap proximity action component back on the building
            // Remove assigned proximity action (worker info UI)
            if (station.AssignedProximityAction != null)
            {
                var assignedComponent = assignedBuilding.GetComponentInstance<ProximityActionComponentInstance>();
                if (assignedComponent != null && assignedComponent.Definition == station.AssignedProximityAction)
                {
                    assignedBuilding.RemoveComponent(assignedComponent);
                }
            }

            // Add back unassigned proximity action (assignment UI)
            if (station.UnassignedProximityAction != null)
            {
                var unassignedComponent = station.UnassignedProximityAction.CreateInstance(assignedBuilding);
                assignedBuilding.ComponentInstances.Add(unassignedComponent);
                unassignedComponent.OnInitialize();
            }

            // Clear assigned unit reference
            station.SetAssignedUnit(null);
        }

        // Save state
        if (unitInstanceService != null)
        {
            unitInstanceService.SaveData();
        }
    }
}
