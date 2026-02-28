using System;
using UnityEngine;

public class UnitRosterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkRunPartyService partyService;
    [SerializeField] private ListUI<UnitRosterItemUI> partyList;

    public event Action<UnitInstance> OnUnitSelected;

    private void Awake()
    {
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
            item.OnUnitClicked -= HandleUnitClicked;
            item.OnUnitClicked += HandleUnitClicked;
        });
    }

    private void HandleUnitClicked(UnitInstance unit)
    {
        OnUnitSelected?.Invoke(unit);
    }
}
