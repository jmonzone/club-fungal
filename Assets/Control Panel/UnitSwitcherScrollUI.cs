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
    [SerializeField] private UnityEngine.UI.Button leftButton;
    [SerializeField] private UnityEngine.UI.Button rightButton;

    [Header("Runtime")]
    [SerializeField] private UnitInstance currentlySelectedUnit;

    public event UnityAction<UnitInstance> OnUnitSelected;

    private void Awake()
    {
        if (unitList != null)
        {
            unitList.Initialize(transform);
        }

        if (leftButton != null)
        {
            leftButton.onClick.AddListener(CyclePrevious);
        }

        if (rightButton != null)
        {
            rightButton.onClick.AddListener(CycleNext);
        }
    }

    private void OnDestroy()
    {
        if (leftButton != null)
        {
            leftButton.onClick.RemoveListener(CyclePrevious);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveListener(CycleNext);
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

    private void CyclePrevious()
    {
        CycleUnit(1);
    }

    private void CycleNext()
    {
        CycleUnit(-1);
    }

    private void CycleUnit(int direction)
    {
        if (partyService == null || partyService.Party == null || partyService.Party.Units == null)
            return;

        var units = partyService.Party.Units;
        if (units.Count == 0) return;

        int currentIndex = units.IndexOf(currentlySelectedUnit);
        if (currentIndex == -1)
        {
            // No selection, start at first
            currentIndex = 0;
        }
        else
        {
            // Cycle with wrapping
            currentIndex = (currentIndex + direction + units.Count) % units.Count;
        }

        var newUnit = units[currentIndex];
        currentlySelectedUnit = newUnit;
        UpdateList();
        OnUnitSelected?.Invoke(newUnit);
    }
}
