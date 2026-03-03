using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI component for manual control mode.
/// Managed by ControlPanelUI.
/// </summary>
public class UnitControllerUI : ControlPanelViewUI
{
    public enum UnitSwitcherMode
    {
        ShoulderButtons,
        ScrollView
    }

    [Header("Settings")]
    [SerializeField] private UnitSwitcherMode switcherMode = UnitSwitcherMode.ShoulderButtons;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    [Header("Unit Switcher Components")]
    [SerializeField] private ShoulderButtonsUI shoulderButtons;
    [SerializeField] private UnitSwitcherScrollUI scrollView;

    private ControlModeService controlModeService;

    public override ControlModeService.ControlMode[] SupportedModes => new[] { ControlModeService.ControlMode.ManualControl };

    public override void Initialize(ControlModeService controlModeService, UnitControllerService unitControllerService)
    {
        base.Initialize(controlModeService, unitControllerService);
        this.controlModeService = controlModeService;

        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackClicked);
        }

        if (shoulderButtons != null)
        {
            shoulderButtons.OnCyclePrevious += HandleCyclePrevious;
            shoulderButtons.OnCycleNext += HandleCycleNext;
        }

        if (scrollView != null)
        {
            scrollView.OnUnitSelected += HandleUnitSelected;
        }

        UpdateSwitcherMode();
    }

    public override void OnShown()
    {
        base.OnShown();
        UpdateSwitcher();
    }

    public override void Cleanup()
    {
        base.Cleanup();

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackClicked);
        }

        if (shoulderButtons != null)
        {
            shoulderButtons.OnCyclePrevious -= HandleCyclePrevious;
            shoulderButtons.OnCycleNext -= HandleCycleNext;
        }

        if (scrollView != null)
        {
            scrollView.OnUnitSelected -= HandleUnitSelected;
        }
    }

    private void HandleBackClicked()
    {
        controlModeService?.StopManualControl();
    }

    private void HandleCyclePrevious()
    {
        controlModeService?.CyclePreviousUnit();
        UpdateSwitcher();
    }

    private void HandleCycleNext()
    {
        controlModeService?.CycleNextUnit();
        UpdateSwitcher();
    }

    private void HandleUnitSelected(UnitInstance unit)
    {
        controlModeService?.SelectUnit(unit);
    }

    private void UpdateSwitcher()
    {
        if (controlModeService == null) return;

        var previousUnit = controlModeService.GetPreviousUnit();
        var nextUnit = controlModeService.GetNextUnit();
        var selectedUnit = controlModeService.SelectedUnit;

        UpdateShoulderButtons(previousUnit, nextUnit);
        UpdateScrollView(selectedUnit);
    }

    private void UpdateSwitcherMode()
    {
        if (shoulderButtons != null)
        {
            shoulderButtons.gameObject.SetActive(switcherMode == UnitSwitcherMode.ShoulderButtons);
        }

        if (scrollView != null)
        {
            scrollView.gameObject.SetActive(switcherMode == UnitSwitcherMode.ScrollView);
        }
    }

    public void UpdateShoulderButtons(UnitInstance previousUnit, UnitInstance nextUnit)
    {
        if (shoulderButtons != null && switcherMode == UnitSwitcherMode.ShoulderButtons)
        {
            shoulderButtons.UpdateIcons(previousUnit, nextUnit);
        }
    }

    public void UpdateScrollView(UnitInstance selectedUnit)
    {
        if (scrollView != null && switcherMode == UnitSwitcherMode.ScrollView)
        {
            scrollView.RefreshList(selectedUnit);
        }
    }
}
