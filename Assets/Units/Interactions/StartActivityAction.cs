
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[Serializable]
public class StartActivityAction : InteractionAction
{
    [SerializeField] private ActivityReference activityReference;
    [SerializeField] private PlayerActivityReference playerActivityReference;

    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete)
    {
        var origin = target.transform.position;
        var activity = UnityEngine.Object.Instantiate(activityReference);
        activity.StartActivity(origin, new List<UnitController> { target });

        playerActivityReference.EnterActivity(activity);
        activity.OnActivityHasEnded += () =>
        {
            onComplete?.Invoke();

            // todo: add logic to select specific unit
            // todo: add logic to make this conditional
            // use InteractionSsytem;
            // var targetUnit = unitsToRemove[0];
            // dialogueReference.StartImmediateChat(
            //     unit: targetUnit.Controller,
            //     // dialogue: new Dialogue("I just invited my friend, they should be coming by here soon."),
            //     onComplete: () =>
            //     {
            //         unitControllerService.InviteFriend(targetUnit.Controller);
            //     });
        };
    }
}
