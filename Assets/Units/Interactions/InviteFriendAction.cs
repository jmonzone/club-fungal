
using UnityEngine;
using UnityEngine.Events;

public class InviteFriendAction : InteractionAction
{
    public override void Execute(UnitController source, UnitController target, UnityAction onComplete)
    {
        // Implement your action logic here
        onComplete?.Invoke();
    }
}
