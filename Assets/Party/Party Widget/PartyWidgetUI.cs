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
        unitUIInstances = new List<PartyWidgetUnitUI>(GetComponentsInChildren<PartyWidgetUnitUI>(true));
        foreach (var unitUI in unitUIInstances)
        {
            unitUI.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        UpdatePartyList();
    }

    private void OnEnable()
    {
        partyService.OnUnitAddedToParty += PartyService_OnUnitAddedToParty;
        partyService.OnUnitRemovedFromParty += PartyService_OnUnitRemovedFromParty;
        UpdatePartyList();
    }

    private void OnDisable()
    {
        partyService.OnUnitAddedToParty -= PartyService_OnUnitAddedToParty;
        partyService.OnUnitRemovedFromParty -= PartyService_OnUnitRemovedFromParty;
    }

    private void PartyService_OnUnitAddedToParty(UnitController unit)
    {
        UpdatePartyList();
    }

    private void PartyService_OnUnitRemovedFromParty(UnitController unit)
    {
        UpdatePartyList();
    }

    private void UpdatePartyList()
    {
        int memberCount = partyService.PartyMembers.Count;

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
            unitUI.SetUnit(partyService.PartyMembers[i]);
        }

        // Disable extra instances
        for (int i = memberCount; i < unitUIInstances.Count; i++)
        {
            unitUIInstances[i].gameObject.SetActive(false);
        }
    }
}