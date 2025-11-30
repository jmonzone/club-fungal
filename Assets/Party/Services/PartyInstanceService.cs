using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

[CreateAssetMenu(fileName = "PartyInstanceService", menuName = "Club Fungal/Party/Party Instance Service")]
public class PartyInstanceService : GURUService
{
    [Header("References")]
    [SerializeField] private PlayerReference playerReference;
    [SerializeField] private LocalData localData;
    [SerializeField] private UnitInstanceService unitInstanceService;

    [Header("Runtime")]
    [SerializeField] private List<UnitInstance> partyInstances = new List<UnitInstance>();

    public List<UnitInstance> PartyInstances => partyInstances;
    public event UnityAction<UnitInstance> OnUnitInstanceAddedToParty;
    public event UnityAction<UnitInstance> OnUnitInstanceRemovedFromParty;

    protected override void OnInitialize()
    {
        partyInstances = new List<UnitInstance>();

        // Load saved party
        if (localData.JsonFile.ContainsKey("party"))
        {
            var partyArray = localData.JsonFile["party"] as JArray;
            if (partyArray != null)
            {
                var partyIds = partyArray.Select(t => t.ToString()).ToList();
                foreach (var id in partyIds)
                {
                    var unit = unitInstanceService.Instances.Find(u => u.Id == id);
                    if (unit != null)
                    {
                        // Debug.Log($"Loaded party member: {unit.DisplayName} (ID: {unit.Id})");
                        partyInstances.Add(unit);
                    }
                }
            }
        }

        if (partyInstances.Count == 0 && playerReference.PlayerInstance != null)
        {
            Debug.Log("No saved party found. Adding player to party by default.");
            partyInstances.Add(playerReference.PlayerInstance);
            SaveParty();
        }
    }

    public void AddToParty(UnitController unit)
    {
        if (!partyInstances.Any(p => p.Id == unit.Instance.Id))
        {
            partyInstances.Add(unit.Instance);
            OnUnitInstanceAddedToParty?.Invoke(unit.Instance);
            SaveParty();
        }
    }

    public void RemoveUnitInstanceFromParty(UnitInstance unit)
    {
        var toRemove = partyInstances.FirstOrDefault(p => p.Id == unit.Id);
        if (toRemove != null && toRemove != playerReference.PlayerInstance)
        {
            partyInstances.Remove(toRemove);
            OnUnitInstanceRemovedFromParty?.Invoke(toRemove);
            SaveParty();
        }
    }

    public void AddUnitInstanceToParty(UnitInstance unit)
    {
        if (!partyInstances.Any(p => p.Id == unit.Id))
        {
            partyInstances.Add(unit);
            OnUnitInstanceAddedToParty?.Invoke(unit);
            SaveParty();
        }
    }

    private void SaveParty()
    {
        if (!Application.isPlaying)
        {
            var widgets = FindObjectsByType<PartyWidgetUI>(FindObjectsSortMode.None);
            foreach (var widget in widgets)
            {
                widget.UpdatePartyList();
            }
        }
        var partyIds = partyInstances.Where(u => u != null).Select(u => u.Id).ToList();
        Debug.Log("Saving party: " + string.Join(", ", partyIds));
        localData.SaveData("party", new JArray(partyIds));
    }
}