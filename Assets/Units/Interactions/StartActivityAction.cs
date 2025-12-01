
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[Serializable]
public class StartActivityAction : InteractionAction
{
    [SerializeField] private ActivityReference activityReference;
    [SerializeField] private PlayerActivityReference playerActivityReference;

    public override void Execute(UnitController source, UnitController target, UnityAction onComplete)
    {
        var origin = target.transform.position;
        var activity = UnityEngine.Object.Instantiate(activityReference);
        activity.StartActivity(origin, new List<UnitController> { target });

        playerActivityReference.EnterActivity(activity);
        activity.OnActivityHasEnded += () =>
        {
            onComplete?.Invoke();

            //         unitControllerService.InviteFriend(targetUnit.Controller);

        };
    }
}
