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

    [Header("Assignment Settings")]
    [SerializeField] private UnitComponentDefinition componentToApply;

    public UnitControllerService UnitControllerService => unitControllerService;
    public UnitInstanceService UnitInstanceService => unitInstanceService;
    public UnitComponentDefinition ComponentToApply => componentToApply;

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

        // Hide UI via service
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

        // Get assignment transform from building controller
        Transform assignmentTransform = buildingController.transform;

        // Check if building has a custom assignment transform
        if (buildingController is CookingStationController cookingStation)
        {
            assignmentTransform = cookingStation.GetAssignmentTransform();
        }

        workerUnit.Teleport(assignmentTransform.position, assignmentTransform.parent);

        // Apply component to worker (e.g., cooking component)
        if (componentToApply != null)
        {
            var componentInstance = componentToApply.CreateInstance(workerUnit);
            workerUnit.ComponentInstances.Add(componentInstance);
            componentInstance.OnInitialize();
        }

        // Get the proximity action component from the building and update it
        var proximityComponent = buildingController.GetComponentInstance<ProximityActionComponentInstance>();
        if (proximityComponent != null)
        {
            proximityComponent.SetAssignedUnit(workerUnit);
        }

        // Save state
        if (unitInstanceService != null)
        {
            unitInstanceService.SaveData();
        }
    }
}
