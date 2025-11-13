using UnityEngine;
using UnityEngine.Events;

public enum ShruneReadingState
{
    NULL,
    SETUP,
    CARD,
    REVEAL,
}

public class ShruneReadingActivityController : ActivityController<ShruneReadingUnit>
{
    [SerializeField] private ShruneReadingState currentState;

    public event UnityAction<ShruneReadingState> OnStateChanged;

    protected override void OnPlayerEnter(ActivityUnit player)
    {
        base.OnPlayerEnter(player);
        SetState(ShruneReadingState.SETUP);
    }

    protected override void OnPlayerExit(ActivityUnit player)
    {
        base.OnPlayerExit(player);
        SetState(ShruneReadingState.NULL);
    }

    private void SetState(ShruneReadingState state)
    {
        currentState = state;
        OnStateChanged?.Invoke(state);
    }

    public void StartReading()
    {
        SetState(ShruneReadingState.CARD);
    }

    public void RevealCard()
    {
        SetState(ShruneReadingState.REVEAL);
    }
}
