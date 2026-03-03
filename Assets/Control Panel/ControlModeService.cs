using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Service that manages control modes: Free Camera, Unit Selected, and Manual Control.
/// Both camera and UI components listen to this service.
/// </summary>
[CreateAssetMenu(fileName = "ControlModeService", menuName = "Club Fungal/Control Mode Service")]
public class ControlModeService : GURUService
{
    public enum ControlMode
    {
        FreeCamera,      // Camera pans freely, UI hidden, no unit selected
        UnitSelected,    // Unit selected, control panel shows unit detail UI
        ManualControl    // Manual control mode active, virtual joystick shown
    }

    [Header("Settings")]
    [SerializeField] private bool autoEnterManualControl = false;
    [SerializeField] private bool autoSelectFirstUnit = false;

    [Header("References")]
    [SerializeField] private NetworkRunPartyService partyService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private UnitBehaviourPriorityService behaviourPriorityService;

    [Header("Current State")]
    [SerializeField] private ControlMode currentMode = ControlMode.FreeCamera;
    [SerializeField] private ControlMode previousMode = ControlMode.FreeCamera;
    [SerializeField] private UnitInstance selectedUnit;

    public ControlMode CurrentMode => currentMode;
    public UnitInstance SelectedUnit => selectedUnit;
    public bool AutoEnterManualControl => autoEnterManualControl;

    public event UnityAction<ControlMode> OnModeChanged;
    public event UnityAction<UnitInstance> OnUnitSelected;
    public event UnityAction OnUnitDeselected;
    public event UnityAction<UnitController, AssignUnitAction> OnShowAssignUnitUI;
    public event UnityAction OnHideAssignUnitUI;

    protected override void OnInitialize()
    {
        currentMode = ControlMode.FreeCamera;
        selectedUnit = null;
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        if (autoSelectFirstUnit && partyService != null && partyService.Party != null)
        {
            var partyUnits = partyService.Party.Units;
            if (partyUnits != null && partyUnits.Count > 0)
            {
                SelectUnit(partyUnits[0]);
            }
        }
    }

    public void SelectUnit(UnitInstance unit)
    {
        selectedUnit = unit;

        // Check if unit is assigned to work - they should never enter manual control
        bool isAssignedToWork = false;
        if (unitControllerService != null && behaviourPriorityService != null)
        {
            var controller = unitControllerService.Controllers.Find(c => c.Instance == unit);
            if (controller != null)
            {
                isAssignedToWork = behaviourPriorityService.IsAssignedToWork(controller);
            }
        }

        // If already in manual control, stay in manual control with new unit
        // Otherwise, check if auto-enter is enabled (but not for assigned units)
        if (currentMode != ControlMode.ManualControl)
        {
            if (autoEnterManualControl && !isAssignedToWork)
            {
                TransitionToMode(ControlMode.ManualControl);
            }
            else
            {
                TransitionToMode(ControlMode.UnitSelected);
            }
        }
        else if (isAssignedToWork)
        {
            // If we're in manual control but selecting an assigned unit, go to UnitSelected mode
            TransitionToMode(ControlMode.UnitSelected);
        }

        OnUnitSelected?.Invoke(unit);
    }

    public void DeselectUnit()
    {
        selectedUnit = null;
        TransitionToMode(ControlMode.FreeCamera);
        OnUnitDeselected?.Invoke();
    }

    public void StartManualControl()
    {
        TransitionToMode(ControlMode.ManualControl);
    }

    public void StopManualControl()
    {
        // Return to the mode we were in before entering manual control
        TransitionToMode(previousMode);
    }

    private void TransitionToMode(ControlMode newMode)
    {
        if (currentMode == newMode) return;
        previousMode = currentMode;
        currentMode = newMode;
        OnModeChanged?.Invoke(newMode);
    }

    public bool IsFreeCameraMode() => currentMode == ControlMode.FreeCamera;
    public bool HasUnitSelected() => currentMode == ControlMode.UnitSelected || currentMode == ControlMode.ManualControl;
    public bool IsManualControlActive() => currentMode == ControlMode.ManualControl;

    public void ToggleManualControl()
    {
        if (currentMode == ControlMode.ManualControl)
            StopManualControl();
        else if (currentMode == ControlMode.UnitSelected)
            StartManualControl();
    }

    public void CycleNextUnit()
    {
        if (partyService == null || partyService.Party == null) return;

        var partyUnits = partyService.Party.Units;
        if (partyUnits == null || partyUnits.Count <= 1) return;

        int currentIndex = partyUnits.IndexOf(selectedUnit);
        if (currentIndex == -1) return;

        int nextIndex = (currentIndex + 1) % partyUnits.Count;
        SelectUnit(partyUnits[nextIndex]);
    }

    public void CyclePreviousUnit()
    {
        if (partyService == null || partyService.Party == null) return;

        var partyUnits = partyService.Party.Units;
        if (partyUnits == null || partyUnits.Count <= 1) return;

        int currentIndex = partyUnits.IndexOf(selectedUnit);
        if (currentIndex == -1) return;

        int previousIndex = (currentIndex - 1 + partyUnits.Count) % partyUnits.Count;
        SelectUnit(partyUnits[previousIndex]);
    }

    public UnitInstance GetNextUnit()
    {
        if (partyService == null || partyService.Party == null) return null;

        var partyUnits = partyService.Party.Units;
        if (partyUnits == null || partyUnits.Count <= 1) return null;

        int currentIndex = partyUnits.IndexOf(selectedUnit);
        if (currentIndex == -1) return null;

        int nextIndex = (currentIndex + 1) % partyUnits.Count;
        return partyUnits[nextIndex];
    }

    public UnitInstance GetPreviousUnit()
    {
        if (partyService == null || partyService.Party == null) return null;

        var partyUnits = partyService.Party.Units;
        if (partyUnits == null || partyUnits.Count <= 1) return null;

        int currentIndex = partyUnits.IndexOf(selectedUnit);
        if (currentIndex == -1) return null;

        int previousIndex = (currentIndex - 1 + partyUnits.Count) % partyUnits.Count;
        return partyUnits[previousIndex];
    }

    /// <summary>
    /// Shows the assign unit UI for a building.
    /// Called by AssignUnitAction when a building's proximity action is triggered.
    /// </summary>
    public void ShowAssignUnitUI(UnitController building, AssignUnitAction action)
    {
        OnShowAssignUnitUI?.Invoke(building, action);
    }

    /// <summary>
    /// Hides the assign unit UI.
    /// Called by AssignUnitAction after a unit is assigned.
    /// </summary>
    public void HideAssignUnitUI()
    {
        OnHideAssignUnitUI?.Invoke();
    }
}
