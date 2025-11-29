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

    public event UnityAction<UnitController> OnUnitAddedToParty;
    public event UnityAction<UnitController> OnUnitRemovedFromParty;

    public void Initialize()
    {
        partyMembers = new List<UnitController> { playerReference.Player };
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
}