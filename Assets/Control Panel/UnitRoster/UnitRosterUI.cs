using System;
using UnityEngine;

public class UnitRosterUI : ControlPanelViewUI
{
    [Header("References")]
    [SerializeField] private NetworkRunPartyService partyService;
    [SerializeField] private ListUI<UnitRosterItemUI> partyList;

    [Header("Runtime")]
    [SerializeField] private UnitInstance selectedUnit;

    private ControlModeService controlModeService;

    public override ControlModeService.ControlMode[] SupportedModes => new[] { ControlModeService.ControlMode.FreeCamera };

    public override void Initialize(ControlModeService controlModeService, UnitControllerService unitControllerService)
    {
        base.Initialize(controlModeService, unitControllerService);
        this.controlModeService = controlModeService;
        partyList.Initialize(transform);
    }

    private void Start()
    {
        UpdateRoster();
    }

    private void OnEnable()
    {
        UpdateRoster();
    }

    public void UpdateRoster()
    {
        partyList.UpdateList(partyService?.Party?.Units, (item, unit) =>
        {
            item.SetUnit(unit);
            item.SetSelected(unit == selectedUnit);
            item.OnUnitClicked -= HandleUnitClicked;
            item.OnUnitClicked += HandleUnitClicked;
        });
    }

    private void HandleUnitClicked(UnitInstance unit)
    {
        controlModeService?.SelectUnit(unit);
    }

    public void SetSelectedUnit(UnitInstance unit)
    {
        selectedUnit = unit;
        UpdateRoster();
    }
}
