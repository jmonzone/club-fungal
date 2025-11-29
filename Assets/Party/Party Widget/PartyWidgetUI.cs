using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyWidgetUI : MonoBehaviour
{
    [SerializeField] private PartyService partyService;
    [SerializeField] private Transform unitContainer;
    [SerializeField] private PartyWidgetUnitUI unitUIPrefab;

    private List<PartyWidgetUnitUI> unitUIInstances = new List<PartyWidgetUnitUI>();

    private void Awake()
    {
        unitUIInstances = new List<PartyWidgetUnitUI>();
    }

    private void Start()
    {
        UpdatePartyList();
    }

    private void OnEnable()
    {
        partyService.OnUnitAddedToParty += PartyService_OnUnitAddedToParty;
        partyService.OnUnitRemovedFromParty += PartyService_OnUnitRemovedFromParty;
        partyService.OnUnitInstanceAddedToParty += PartyService_OnUnitInstanceAddedToParty;
        partyService.OnUnitInstanceRemovedFromParty += PartyService_OnUnitInstanceRemovedFromParty;
        UpdatePartyList();
    }

    private void OnDisable()
    {
        partyService.OnUnitAddedToParty -= PartyService_OnUnitAddedToParty;
        partyService.OnUnitRemovedFromParty -= PartyService_OnUnitRemovedFromParty;
        partyService.OnUnitInstanceAddedToParty -= PartyService_OnUnitInstanceAddedToParty;
        partyService.OnUnitInstanceRemovedFromParty -= PartyService_OnUnitInstanceRemovedFromParty;
    }

    private void PartyService_OnUnitAddedToParty(UnitController unit)
    {
        UpdatePartyList();
    }

    private void PartyService_OnUnitRemovedFromParty(UnitController unit)
    {
        UpdatePartyList();
    }

    private void PartyService_OnUnitInstanceAddedToParty(UnitInstance unit)
    {
        UpdatePartyList();
    }

    private void PartyService_OnUnitInstanceRemovedFromParty(UnitInstance unit)
    {
        UpdatePartyList();
    }

    public void UpdatePartyList()
    {
        unitUIInstances = new List<PartyWidgetUnitUI>(GetComponentsInChildren<PartyWidgetUnitUI>(includeInactive: true));

        int memberCount = partyService.PartyInstances.Count;

        // Ensure we have enough instances
        while (unitUIInstances.Count < memberCount)
        {
            var instance = Instantiate(unitUIPrefab, unitContainer);
            unitUIInstances.Add(instance);
        }

        // Update existing instances
        for (int i = 0; i < memberCount; i++)
        {
            var unitUI = unitUIInstances[i];
            unitUI.gameObject.SetActive(true);
            unitUI.SetUnit(partyService.PartyInstances[i]);
        }

        // Disable extra instances
        for (int i = memberCount; i < unitUIInstances.Count; i++)
        {
            unitUIInstances[i].gameObject.SetActive(false);
        }
    }
}