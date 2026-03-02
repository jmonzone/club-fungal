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
    [SerializeField] private Vector3 assignmentAnchorOffset = new Vector3(0, 0, 1f);
    [SerializeField] private UnitComponentDefinition componentToApply;

    public UnitControllerService UnitControllerService => unitControllerService;
    public UnitInstanceService UnitInstanceService => unitInstanceService;
    public Vector3 AssignmentAnchorOffset => assignmentAnchorOffset;
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

        // Teleport worker to building position with offset
        Vector3 assignmentPosition = buildingController.transform.position + assignmentAnchorOffset;
        workerUnit.Teleport(assignmentPosition, buildingController.transform.parent);

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
