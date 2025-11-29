using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

[CreateAssetMenu(fileName = "PartyService", menuName = "Club Fungal/Party/Party Service")]
public class PartyService : ScriptableObject
{
    [Header("References")]
    [SerializeField] private PlayerReference playerReference;
    [SerializeField] private LocalData localData;
    [SerializeField] private UnitInstanceService unitInstanceService;

    [Header("Runtime")]
    [SerializeField] private List<UnitInstance> partyInstances = new List<UnitInstance>();
    [SerializeField] private List<UnitController> partyControllers = new List<UnitController>();

    public PlayerReference PlayerReference => playerReference;
    public List<UnitInstance> PartyInstances => partyInstances;
    public List<UnitController> PartyControllers => partyControllers;
    public event UnityAction<UnitController> OnUnitAddedToParty;
    public event UnityAction<UnitController> OnUnitRemovedFromParty;
    public event UnityAction<UnitInstance> OnUnitInstanceAddedToParty;
    public event UnityAction<UnitInstance> OnUnitInstanceRemovedFromParty;

    public void Initialize()
    {
        localData.Initialize();
        Debug.Log("Loading party from local data.");
        partyInstances = new List<UnitInstance>();
        partyControllers = new List<UnitController>();

        // Load saved party
        if (localData.JsonFile.ContainsKey("party"))
        {
            var partyArray = localData.JsonFile["party"] as JArray;
            if (partyArray != null)
            {
                var partyIds = partyArray.Select(t => t.ToString()).ToList();
                foreach (var id in partyIds)
                {
                    var unit = unitInstanceService.Units.Find(u => u.Id == id);
                    if (unit != null)
                    {
                        Debug.Log($"Loaded party member: {unit.DisplayName} (ID: {unit.Id})");
                        partyInstances.Add(unit);
                        unit.IsInParty = true;
                    }
                }
            }
        }

        if (partyInstances.Count == 0 && playerReference.PlayerInstance != null)
        {
            Debug.Log("No saved party found. Adding player to party by default.");
            partyInstances.Add(playerReference.PlayerInstance);
            playerReference.PlayerInstance.IsInParty = true;
            SaveParty();
        }
    }

    public void AddToParty(UnitController unit)
    {
        Debug.Log($"Adding unit to party: {unit.Instance.DisplayName} (ID: {unit.Instance.Id})");
        if (!partyControllers.Contains(unit))
        {
            partyControllers.Add(unit);
            var followBehaviour = unit.GetComponent<UnitFollow>();
            if (followBehaviour != null)
            {
                followBehaviour.SetTarget(playerReference.Player.transform);
                unit.SetBehaviour(followBehaviour);
            }
            if (!partyInstances.Any(p => p.Id == unit.Instance.Id))
            {
                partyInstances.Add(unit.Instance);
                unit.Instance.IsInParty = true;
                OnUnitInstanceAddedToParty?.Invoke(unit.Instance);
            }
            OnUnitAddedToParty?.Invoke(unit);
            SaveParty();
        }
    }

    public void RemoveUnitInstanceFromParty(UnitInstance unit)
    {
        var toRemove = partyInstances.FirstOrDefault(p => p.Id == unit.Id);
        if (toRemove != null && toRemove != playerReference.PlayerInstance)
        {
            partyInstances.Remove(toRemove);
            toRemove.IsInParty = false;
            OnUnitInstanceRemovedFromParty?.Invoke(toRemove);
            SaveParty();
        }
    }

    public void AddUnitInstanceToParty(UnitInstance unit)
    {
        if (!partyInstances.Any(p => p.Id == unit.Id))
        {
            partyInstances.Add(unit);
            unit.IsInParty = true;
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