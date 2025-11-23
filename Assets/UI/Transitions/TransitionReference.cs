using UnityEngine;
using UnityEngine.Events;

public enum TransitionType
{
    CIRCLE_IN,
    CIRCLE_OUT,
}

[CreateAssetMenu(fileName = "TransitionReference", menuName = "Club Fungal/UI/Transition Reference")]
public class TransitionReference : ScriptableObject
{
    public event UnityAction<TransitionType, UnityAction> OnTransitionStarted;

    public void StartTransition(TransitionType type, UnityAction onComplete = null)
    {
        OnTransitionStarted?.Invoke(type, onComplete);
    }
}
