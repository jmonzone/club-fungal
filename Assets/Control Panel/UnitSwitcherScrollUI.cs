using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Scrollable masked panel of units for switching between party members.
/// </summary>
public class UnitSwitcherScrollUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkRunPartyService partyService;
    [SerializeField] private ListUI<UnitRosterItemUI> unitList;

    [Header("Runtime")]
    [SerializeField] private UnitInstance currentlySelectedUnit;

    public event UnityAction<UnitInstance> OnUnitSelected;

    private void Awake()
    {
        if (unitList != null)
        {
            unitList.Initialize(transform);
        }
    }

    private void OnEnable()
    {
        UpdateList();
    }

    public void UpdateList()
    {
        if (partyService == null || unitList == null) return;

        unitList.UpdateList(partyService?.Party?.Units, (item, unit) =>
        {
            item.SetUnit(unit);
            item.SetSelected(unit == currentlySelectedUnit);
            item.OnUnitClicked -= HandleUnitClicked;
            item.OnUnitClicked += HandleUnitClicked;
        });
    }

    private void HandleUnitClicked(UnitInstance unit)
    {
        OnUnitSelected?.Invoke(unit);
    }

    public void RefreshList(UnitInstance selectedUnit)
    {
        currentlySelectedUnit = selectedUnit;
        UpdateList();
    }
}
