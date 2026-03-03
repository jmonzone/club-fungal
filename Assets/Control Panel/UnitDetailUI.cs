using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailUI : ControlPanelViewUI
{
    [Header("Services")]
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private UnitBehaviourPriorityService behaviourPriorityService;

    [Header("UI References")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button manualControlButton;
    [SerializeField] private Button unassignButton;
    [SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI speciesText;
    [SerializeField] private TextMeshProUGUI jobText;

    [Header("Runtime")]
    [SerializeField] private UnitInstance unitInstance;

    private ControlModeService controlModeService;

    public override ControlModeService.ControlMode[] SupportedModes => new[] { ControlModeService.ControlMode.UnitSelected };
    public UnitInstance UnitInstance => unitInstance;

    public override void Initialize(ControlModeService controlModeService, UnitControllerService unitControllerService)
    {
        base.Initialize(controlModeService, unitControllerService);
        this.controlModeService = controlModeService;

        if (backButton)
        {
            backButton.onClick.AddListener(HandleBackClicked);
        }

        if (manualControlButton)
        {
            manualControlButton.onClick.AddListener(HandleManualControlClicked);
        }

        if (unassignButton)
        {
            unassignButton.onClick.AddListener(HandleUnassignClicked);
        }
    }

    public override void OnShown()
    {
        base.OnShown();
        SetUnit(controlModeService?.SelectedUnit);
    }

    public override void Cleanup()
    {
        base.Cleanup();

        if (backButton)
        {
            backButton.onClick.RemoveListener(HandleBackClicked);
        }

        if (manualControlButton)
        {
            manualControlButton.onClick.RemoveListener(HandleManualControlClicked);
        }

        if (unassignButton)
        {
            unassignButton.onClick.RemoveListener(HandleUnassignClicked);
        }
    }

    private void HandleBackClicked()
    {
        controlModeService?.DeselectUnit();
    }

    private void HandleManualControlClicked()
    {
        controlModeService?.StartManualControl();
    }

    private void HandleUnassignClicked()
    {
        if (unitInstance == null || unitControllerService == null) return;

        var unitController = unitControllerService.Controllers.Find(c => c.Instance == unitInstance);
        if (unitController == null) return;

        // Find the building this unit is assigned to
        foreach (var controller in unitControllerService.Controllers)
        {
            var proximityComponent = controller.GetComponentInstance<ProximityActionComponentInstance>();
            if (proximityComponent != null && proximityComponent.AssignedUnit == unitController)
            {
                var proximityDef = proximityComponent.Definition as ProximityActionComponentDefinition;
                if (proximityDef?.Action is AssignUnitAction assignAction)
                {
                    assignAction.UnassignUnit(unitController);
                    controlModeService?.DeselectUnit();
                    break;
                }
            }
        }
    }

    public void SetUnit(UnitInstance unit)
    {
        unitInstance = unit;

        if (unit != null)
        {
            // Check if unit is assigned to work
            bool isAssignedToWork = false;
            if (unitControllerService != null && behaviourPriorityService != null)
            {
                var controller = unitControllerService.Controllers.Find(c => c.Instance == unit);
                if (controller != null)
                {
                    isAssignedToWork = behaviourPriorityService.IsAssignedToWork(controller);
                }
            }

            // Show/hide buttons based on assignment status
            if (manualControlButton)
            {
                manualControlButton.gameObject.SetActive(!isAssignedToWork);
            }

            if (unassignButton)
            {
                unassignButton.gameObject.SetActive(isAssignedToWork);
            }

            if (nameText)
            {
                nameText.text = unit.DisplayName;
            }

            if (speciesText && unit.Species)
            {
                speciesText.text = unit.Species.name;
            }

            if (jobText && unit.Job)
            {
                jobText.text = unit.Job.ActionName;
            }

            if (unitImage && unit.Species)
            {
                unitImage.sprite = unit.Species.Sprite;
                unitImage.enabled = true;
            }
        }
        else
        {
            if (nameText)
            {
                nameText.text = "";
            }

            if (speciesText)
            {
                speciesText.text = "";
            }

            if (jobText)
            {
                jobText.text = "";
            }

            if (unitImage)
            {
                unitImage.sprite = null;
                unitImage.enabled = false;
            }
        }
    }
}
