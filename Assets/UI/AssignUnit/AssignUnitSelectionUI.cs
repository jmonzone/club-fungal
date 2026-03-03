using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UI for selecting a unit to assign to a building/station.
/// Shows all available units sorted by suitability.
/// </summary>
public class AssignUnitSelectionUI : ControlPanelViewUI
{
    [Header("Services")]
    [SerializeField] private NetworkRunPartyService networkRunPartyService;

    [Header("UI References")]
    [SerializeField] private ListUI<AssignUnitItemUI> unitList;

    [Header("Runtime")]
    private UnitController buildingController;
    private AssignUnitAction action;

    // This view is shown via events, not control modes
    public override ControlModeService.ControlMode[] SupportedModes => new ControlModeService.ControlMode[0];

    public override void Initialize(ControlModeService controlModeService, UnitControllerService unitControllerService)
    {
        base.Initialize(controlModeService, unitControllerService);
        unitList.Initialize(transform);
    }

    /// <summary>
    /// Show this view with building and action context.
    /// </summary>
    public void ShowForAssignment(UnitController building, AssignUnitAction action)
    {
        this.buildingController = building;
        this.action = action;
        PopulateUnitList();
        Show();
    }

    private void OnEnable()
    {
        if (buildingController != null && action != null)
        {
            PopulateUnitList();
        }
    }

    private void PopulateUnitList()
    {
        if (networkRunPartyService == null || networkRunPartyService.PartyControllers == null)
        {
            return;
        }

        // Get all party units
        var allUnits = networkRunPartyService.PartyControllers
            .Where(u => u != null && u.Instance != null)
            .ToList();

        // Sort units by suitability
        var sortedUnits = SortUnitsBySuitability(allUnits);

        // Update list using ListUI's pooling system
        unitList.UpdateList(sortedUnits, (itemUI, unit) =>
        {
            var availability = GetUnitAvailability(unit);
            itemUI.Initialize(unit, availability);

            // Subscribe to click event (remove first to avoid duplicates on reused items)
            itemUI.OnUnitClicked -= OnUnitItemClicked;
            itemUI.OnUnitClicked += OnUnitItemClicked;
        });
    }

    /// <summary>
    /// Sort units by suitability: jobless first, then by distance, then by name.
    /// </summary>
    private List<UnitController> SortUnitsBySuitability(List<UnitController> units)
    {
        if (buildingController == null) return units;

        return units.OrderBy(u => u.Instance.Job != null ? 1 : 0) // Jobless first
                    .ThenBy(u => Vector3.Distance(u.transform.position, buildingController.transform.position)) // Closest first
                    .ThenBy(u => u.Instance.DisplayName) // Alphabetical
                    .ToList();
    }

    /// <summary>
    /// Determine unit availability status for visual indicators.
    /// </summary>
    private UnitAvailability GetUnitAvailability(UnitController unit)
    {
        if (unit == null || unit.Instance == null)
        {
            return UnitAvailability.Unavailable;
        }

        // Check if unit already has a job
        if (unit.Instance.Job != null)
        {
            return UnitAvailability.AssignedElsewhere;
        }

        return UnitAvailability.Available;
    }

    private void OnUnitItemClicked(UnitController unit)
    {
        if (unit == null || action == null)
        {
            return;
        }

        Debug.Log($"Selected unit: {unit.Instance.DisplayName}");

        // Call action to handle assignment
        action.SelectUnit(unit);
    }

    private void OnDisable()
    {
        // Clean up event listeners
        foreach (var item in unitList.Items)
        {
            if (item != null)
            {
                item.OnUnitClicked -= OnUnitItemClicked;
            }
        }
    }
}

/// <summary>
/// Availability status for visual indicators.
/// </summary>
public enum UnitAvailability
{
    Available,          // No job, ready to be assigned
    AssignedElsewhere,  // Has a job but can be reassigned
    Unavailable         // Cannot be assigned
}
