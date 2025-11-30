using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class JoinPartyAction : InteractionAction
{
    public override string DisplayName => "Join Party";
    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete)
    {
        partyControllerService.AddToParty(target);
        Debug.Log("Joined party!");
        onComplete();
    }
}