
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class StartActivityAction : InteractionAction
{
    [SerializeField] private ActivityReference activityReference;

    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete)
    {
        // Implement your action logic here
        onComplete?.Invoke();
    }
}
