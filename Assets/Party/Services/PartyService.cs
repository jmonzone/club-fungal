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
    [SerializeField] private List<UnitInstance> partyMembers = new List<UnitInstance>();
    [SerializeField] private List<UnitController> partyControllers = new List<UnitController>();

    public PlayerReference PlayerReference => playerReference;
    public List<UnitController> PartyMembers => partyControllers;
    public event UnityAction<UnitController> OnUnitAddedToParty;
    public event UnityAction<UnitController> OnUnitRemovedFromParty;

    public void Initialize()
    {
        localData.Initialize();
        partyMembers = new List<UnitInstance>();

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
                        partyMembers.Add(unit);
                        unit.IsInParty = true;
                    }
                }
            }
        }

        if (partyMembers.Count == 0 && playerReference.Player != null)
        {
            partyMembers.Add(playerReference.Player.Instance);
            playerReference.Player.Instance.IsInParty = true;
            SaveParty();
        }

        playerReference.OnPlayerSet += (player) =>
        {
            if (!partyMembers.Contains(player.Instance))
            {
                partyMembers.Add(player.Instance);
                player.Instance.IsInParty = true;
            }
        };
    }

    public void AddToParty(UnitController unit)
    {
        if (!partyControllers.Contains(unit))
        {
            partyControllers.Add(unit);
            var followBehaviour = unit.GetComponent<UnitFollow>();
            if (followBehaviour != null)
            {
                followBehaviour.SetTarget(playerReference.Player.transform);
                unit.SetBehaviour(followBehaviour);
            }
            if (!partyMembers.Contains(unit.Instance))
            {
                partyMembers.Add(unit.Instance);
                unit.Instance.IsInParty = true;
            }
            OnUnitAddedToParty?.Invoke(unit);
            SaveParty();
        }
    }

    public void RemoveFromParty(UnitController unit)
    {
        if (partyMembers.Contains(unit.Instance) && unit != playerReference.Player)
        {
            partyMembers.Remove(unit.Instance);
            unit.Instance.IsInParty = false;
            OnUnitRemovedFromParty?.Invoke(unit);
            SaveParty();
        }
    }

    private void SaveParty()
    {
        var partyIds = partyMembers.Where(u => u != null).Select(u => u.Id).ToList();
        localData.SaveData("party", new JArray(partyIds));
    }
}