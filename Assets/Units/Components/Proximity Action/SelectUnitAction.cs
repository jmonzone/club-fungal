using UnityEngine;

/// <summary>
/// Action that selects a unit (shows their detail UI).
/// Used for assigned workers at stations.
/// </summary>
[CreateAssetMenu(fileName = "SelectUnitAction", menuName = "Club Fungal/Unit Actions/Select Unit")]
public class SelectUnitAction : UnitAction
{
    [Header("Services")]
    [SerializeField] private ControlModeService controlModeService;

    public override void Execute(UnitController buildingController)
    {
        if (buildingController == null)
        {
            return;
        }

        // Find the assigned unit from the building's AssignableStation
        var station = buildingController.GetComponent<AssignableStation>();
        if (station != null && station.AssignedUnit != null && station.AssignedUnit.Instance != null)
        {
            if (controlModeService != null)
            {
                controlModeService.SelectUnit(station.AssignedUnit.Instance);
            }
        }
    }
}
