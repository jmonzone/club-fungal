using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "DialogueInteraction", menuName = "Club Fungal/Interactions/Dialogue Interaction")]
public class DialogueInteraction : UnitInteraction
{
    [SerializeField] private DialogueReference dialogueReference;
    [SerializeField] private UnitControllerService unitControllerService;

    [HideInInspector]
    [SerializeReference] private List<InteractionAction> actions;

    private UnitController source;
    private UnitController target;
    private int currentActionIndex = 0;

    public override void StartInteraction(UnitController source, UnitController target)
    {
        this.source = source;
        this.target = target;

        source.Dialogue.StartDialogue(target);
        target.Dialogue.StartDialogue(source);

        currentActionIndex = 0;
        ExecuteNext();
    }

    private void ExecuteNext()
    {
        if (currentActionIndex < actions.Count)
        {
            actions[currentActionIndex].Execute(source, target, dialogueReference, unitControllerService, () =>
            {
                currentActionIndex++;
                ExecuteNext();
            });
        }
    }

    public void ContinueInteraction()
    {
        // Called by dialogue UI continue button to advance to next action
        currentActionIndex++;
        ExecuteNext();
    }
}

[Serializable]
public abstract class InteractionAction
{
    public abstract void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, UnityAction onComplete);
}

[Serializable]
public class DialogueAction : InteractionAction
{
    public enum DialogueSpeaker { Source, Target, Specific }

    [SerializeField] private DialogueSpeaker speaker;
    [SerializeField] private UnitInstance unitInstance;
    [SerializeField][TextArea] private string text;

    public DialogueSpeaker Speaker => speaker;
    public UnitInstance UnitInstance => unitInstance;

    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, UnityAction onComplete)
    {
        var unitInstance = Speaker switch
        {
            DialogueSpeaker.Source => source.Instance,
            DialogueSpeaker.Target => target.Instance,
            _ => UnitInstance
        };

        var dialogue = new Dialogue(unitInstance, text, onComplete);
        dialogueReference.StartDialogue(dialogue);

        // For dialogue, completion is handled by ContinueInteraction when user presses continue
        // Do not call onComplete here

        // Focus on the speaker
        UnitController speakerController = Speaker switch
        {
            DialogueSpeaker.Source => source,
            DialogueSpeaker.Target => target,
            _ => unitControllerService.GetController(UnitInstance)
        };
        speakerController?.Focus();
    }
}

[Serializable]
public class JoinPartyAction : InteractionAction
{
    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, UnityAction onComplete)
    {
        // TODO: implement join party logic
        Debug.Log("Joining party!");
        onComplete();
    }
}