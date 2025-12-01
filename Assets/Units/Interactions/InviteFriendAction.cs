
using UnityEngine;
using UnityEngine.Events;

public class InviteFriendAction : InteractionAction
{
    [SerializeField] private UnitControllerService unitControllerService;

    public override void Execute(UnitController source, UnitController target, UnityAction onComplete)
    {
        unitControllerService.InviteFriend(target);
        onComplete?.Invoke();
    }
}
