
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
    }
}
