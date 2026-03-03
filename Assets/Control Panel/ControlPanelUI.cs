using UnityEngine;
using System;

/// <summary>
/// Manages visibility of UI panels and relays events to ControlModeService
/// </summary>
public class ControlPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ControlModeService controlModeService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private GameObject renderRoot;

    [Header("Views")]
    [SerializeField] private UnitRosterUI unitRosterUI;
    [SerializeField] private UnitDetailUI unitDetailUI;
    [SerializeField] private UnitControllerUI unitControllerUI;
    [SerializeField] private AssignUnitSelectionUI assignUnitSelectionUI;

    private void Awake()
    {
        unitRosterUI.OnUnitSelected += HandleUnitSelected;
        unitDetailUI.OnBackClicked += HandleBackClicked;
        unitDetailUI.OnManualControlClicked += HandleManualControlClicked;
        unitDetailUI.OnUnassignClicked += HandleUnassignClicked;
        unitControllerUI.OnBackClicked += HandleControllerBackClicked;
        unitControllerUI.OnCyclePrevious += HandleCyclePrevious;
        unitControllerUI.OnCycleNext += HandleCycleNext;
        unitControllerUI.OnUnitSelected += HandleUnitSelected; // For scroll view mode

        controlModeService.OnModeChanged += HandleModeChanged;
        controlModeService.OnUnitSelected += HandleUnitSelectedForSwitcher;
        controlModeService.OnShowAssignUnitUI += HandleShowAssignUnitUI;
        controlModeService.OnHideAssignUnitUI += HandleHideAssignUnitUI;
    }

    private void Start()
    {
        HandleModeChanged(controlModeService.CurrentMode);
    }

    private void OnDestroy()
    {
        unitRosterUI.OnUnitSelected -= HandleUnitSelected;
        unitDetailUI.OnBackClicked -= HandleBackClicked;
        unitDetailUI.OnManualControlClicked -= HandleManualControlClicked;
        unitDetailUI.OnUnassignClicked -= HandleUnassignClicked;
        unitControllerUI.OnBackClicked -= HandleControllerBackClicked;
        unitControllerUI.OnCyclePrevious -= HandleCyclePrevious;
        unitControllerUI.OnCycleNext -= HandleCycleNext;
        unitControllerUI.OnUnitSelected -= HandleUnitSelected;

        controlModeService.OnModeChanged -= HandleModeChanged;
        controlModeService.OnUnitSelected -= HandleUnitSelectedForSwitcher;
        controlModeService.OnShowAssignUnitUI -= HandleShowAssignUnitUI;
        controlModeService.OnHideAssignUnitUI -= HandleHideAssignUnitUI;
    }

    private void HandleUnitSelected(UnitInstance unit)
    {
        controlModeService.SelectUnit(unit);
    }

    private void HandleBackClicked()
    {
        controlModeService.DeselectUnit();
    }

    private void HandleManualControlClicked()
    {
        controlModeService.StartManualControl();
    }

    private void HandleUnassignClicked()
    {
        var selectedUnit = unitDetailUI.UnitInstance;
        if (selectedUnit == null) return;

        var unitController = unitControllerService.Controllers.Find(c => c.Instance == selectedUnit);
        if (unitController == null) return;

        // Find the building this unit is assigned to
        foreach (var controller in unitControllerService.Controllers)
        {
            var proximityComponent = controller.GetComponentInstance<ProximityActionComponentInstance>();
            if (proximityComponent != null && proximityComponent.AssignedUnit == unitController)
            {
                // Get the action from the component definition
                var proximityDef = proximityComponent.Definition as ProximityActionComponentDefinition;
                if (proximityDef?.Action is AssignUnitAction assignAction)
                {
                    assignAction.UnassignUnit(unitController);
                    controlModeService.DeselectUnit();
                    break;
                }
            }
        }
    }

    private void HandleControllerBackClicked()
    {
        controlModeService.StopManualControl();
    }

    private void HandleCyclePrevious()
    {
        controlModeService.CyclePreviousUnit();
    }

    private void HandleCycleNext()
    {
        controlModeService.CycleNextUnit();
    }

    private void HandleUnitSelectedForSwitcher(UnitInstance unit)
    {
        // Update unit switcher UI (shoulder buttons or scroll view)
        UpdateUnitSwitcher();
    }

    private void UpdateUnitSwitcher()
    {
        if (controlModeService.IsManualControlActive())
        {
            var previousUnit = controlModeService.GetPreviousUnit();
            var nextUnit = controlModeService.GetNextUnit();
            var selectedUnit = controlModeService.SelectedUnit;

            // Update shoulder buttons with adjacent units
            unitControllerUI.UpdateShoulderButtons(previousUnit, nextUnit);

            // Update scroll view with selected unit
            unitControllerUI.UpdateScrollView(selectedUnit);
        }
    }

    private void HandleModeChanged(ControlModeService.ControlMode mode)
    {
        switch (mode)
        {
            case ControlModeService.ControlMode.FreeCamera:
                ShowRoster();
                break;
            case ControlModeService.ControlMode.UnitSelected:
                ShowUnitDetail(controlModeService.SelectedUnit);
                break;
            case ControlModeService.ControlMode.ManualControl:
                ShowManualControl();
                break;
        }
    }

    private void ShowRoster()
    {
        if (renderRoot != null) renderRoot.SetActive(false);
        unitRosterUI.gameObject.SetActive(true);
        unitDetailUI.gameObject.SetActive(false);
        unitControllerUI.gameObject.SetActive(false);
    }

    private void ShowUnitDetail(UnitInstance unit)
    {
        if (renderRoot != null) renderRoot.SetActive(true);
        unitRosterUI.gameObject.SetActive(false);
        unitDetailUI.SetUnit(unit);
        unitDetailUI.gameObject.SetActive(true);
        unitControllerUI.gameObject.SetActive(false);
    }

    private void ShowManualControl()
    {
        if (renderRoot != null) renderRoot.SetActive(true);
        unitRosterUI.gameObject.SetActive(false);
        unitDetailUI.gameObject.SetActive(false);
        unitControllerUI.gameObject.SetActive(true);
        UpdateUnitSwitcher();
    }

    private void HandleShowAssignUnitUI(UnitController building, AssignUnitAction action)
    {
        if (assignUnitSelectionUI == null)
        {
            return;
        }

        assignUnitSelectionUI.gameObject.SetActive(true);
        assignUnitSelectionUI.Initialize(building, action);
    }

    private void HandleHideAssignUnitUI()
    {
        if (assignUnitSelectionUI == null)
        {
            return;
        }

        // Hide the assign unit UI
        assignUnitSelectionUI.gameObject.SetActive(false);
    }
}
