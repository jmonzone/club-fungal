using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PartyService", menuName = "Club Fungal/Party/Party Service")]
public class PartyService : ScriptableObject
{
    [Header("References")]
    [SerializeField] private PlayerReference playerReference;

    [Header("Runtime")]
    [SerializeField] private List<UnitController> partyMembers = new List<UnitController>();

    public List<UnitController> PartyMembers => partyMembers;
    public event UnityAction<UnitController> OnUnitAddedToParty;
    public event UnityAction<UnitController> OnUnitRemovedFromParty;

    public void Initialize()
    {
        partyMembers = new List<UnitController>();
        playerReference.OnPlayerSet += (player) =>
        {
            partyMembers.Add(player);
        };
    }

    public void AddToParty(UnitController unit)
    {
        if (!partyMembers.Contains(unit))
        {
            partyMembers.Add(unit);
            var followBehaviour = unit.GetComponent<UnitFollow>();
            if (followBehaviour != null)
            {
                followBehaviour.SetTarget(playerReference.Player.transform);
                unit.SetBehaviour(followBehaviour);
            }
            OnUnitAddedToParty?.Invoke(unit);
        }
    }

    public void RemoveFromParty(UnitController unit)
    {
        if (partyMembers.Contains(unit) && unit != playerReference.Player)
        {
            partyMembers.Remove(unit);
            OnUnitRemovedFromParty?.Invoke(unit);
        }
    }
}