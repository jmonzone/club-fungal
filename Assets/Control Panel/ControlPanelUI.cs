using UnityEngine;
using System;

/// <summary>
/// Manages visibility of UI panels and relays events to ControlModeService
/// </summary>
public class ControlPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ControlModeService controlModeService;
    [SerializeField] private GameObject renderRoot;

    [Header("Views")]
    [SerializeField] private UnitRosterUI unitRosterUI;
    [SerializeField] private UnitDetailUI unitDetailUI;
    [SerializeField] private UnitControllerUI unitControllerUI;

    private void Awake()
    {
        unitRosterUI.OnUnitSelected += HandleUnitSelected;
        unitDetailUI.OnBackClicked += HandleBackClicked;
        unitDetailUI.OnManualControlClicked += HandleManualControlClicked;
        unitControllerUI.OnBackClicked += HandleControllerBackClicked;

        controlModeService.OnModeChanged += HandleModeChanged;
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
        unitControllerUI.OnBackClicked -= HandleControllerBackClicked;

        controlModeService.OnModeChanged -= HandleModeChanged;
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

    private void HandleControllerBackClicked()
    {
        controlModeService.StopManualControl();
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
    }
}
