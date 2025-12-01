using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[Serializable]
public class DialogueAction : InteractionAction
{
    public override string DisplayName => "Dialogue";

    public enum DialogueSpeaker
    {
        Source,
        Target,
        Specific,
    }
    [SerializeField] private DialogueSpeaker speaker;
    [SerializeField][TextArea] private string text;
    [SerializeField] private UnitInstance unitInstance;
    [SerializeField] private bool isFirst;

    public DialogueSpeaker Speaker => speaker;
    public UnitInstance UnitInstance => unitInstance;

    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete)
    {
        if (isFirst)
        {
            source.Dialogue.StartDialogue(target);
            target.Dialogue.StartDialogue(source);

            dialogueReference.StartDialogueInteraction(new List<UnitController> { source, target });

        }

        unitInstance = Speaker switch
        {
            DialogueSpeaker.Source => source.Instance,
            DialogueSpeaker.Target => target.Instance,
            _ => throw new ArgumentOutOfRangeException()
        };

        var speakerController = Speaker switch
        {
            DialogueSpeaker.Source => source,
            DialogueSpeaker.Target => target,
            _ => throw new ArgumentOutOfRangeException()
        };

        var dialogue = new Dialogue(unitInstance, text, onComplete);
        dialogueReference.StartDialogue(speakerController, dialogue);
    }
}