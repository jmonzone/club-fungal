using System.Collections;
using UnityEngine;

public class UnitPlayer : UnitBehaviour
{
    [SerializeField] private PlayerService playerReference;

    private Coroutine interactRoutine;

    protected override void OnBehaviourStart()
    {
        playerReference.OnTargetPositionChanged += PlayerReference_OnTargetPositionChanged;
        playerReference.OnTargetInteractableChanged += PlayerReference_OnTargetInteractableChanged;
    }

    public override void StopBehaviour()
    {
        base.StopBehaviour();
        playerReference.OnTargetPositionChanged -= PlayerReference_OnTargetPositionChanged;
        playerReference.OnTargetInteractableChanged -= PlayerReference_OnTargetInteractableChanged;
        if (interactRoutine != null) StopCoroutine(interactRoutine);
    }

    private void PlayerReference_OnTargetInteractableChanged()
    {
        if (playerReference.TargetInteractable != null)
        {
            if (interactRoutine != null) StopCoroutine(interactRoutine);
            interactRoutine = StartCoroutine(InteractRoutine());
        }
    }

    private IEnumerator InteractRoutine()
    {
        Controller.Destination.SetTarget(playerReference.TargetInteractable.Transform);
        yield return new WaitUntil(() => Controller.Destination.IsAtDestination);
        playerReference.TargetInteractable.Select(Controller);
    }

    private void PlayerReference_OnTargetPositionChanged()
    {
        if (playerReference.TargetInteractable == null)
        {
            Controller.SetLookPosition(playerReference.TargetPosition);
            Controller.Destination.SetDestination(playerReference.TargetPosition);
        }
    }
}