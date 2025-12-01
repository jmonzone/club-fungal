using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class JoinPartyAction : InteractionAction
{
    [SerializeField] private PartyControllerService partyControllerService;
    public override string DisplayName => "Join Party";
    public override void Execute(UnitController source, UnitController target, UnityAction onComplete)
    {
        partyControllerService.AddToParty(target);
        Debug.Log("Joined party!");
        onComplete();
    }
}