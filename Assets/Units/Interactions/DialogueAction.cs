using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[Serializable]
public class DialogueActionBlock
{
    public enum DialogueSpeaker
    {
        Source,
        Target,
    }

    [SerializeField] public DialogueSpeaker speaker;
    [SerializeField][TextArea] public string text;
}

[Serializable]
public class DialogueAction : InteractionAction
{
    public override string DisplayName => "Dialogue";

    [SerializeField] private List<DialogueActionBlock> blocks = new();

    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete)
    {
        source.Dialogue.StartDialogue(target);
        target.Dialogue.StartDialogue(source);

        dialogueReference.StartDialogueInteraction(new List<UnitController> { source, target });

        foreach (var block in blocks)
        {
            var unitInstance = block.speaker switch
            {
                DialogueActionBlock.DialogueSpeaker.Source => source.Instance,
                DialogueActionBlock.DialogueSpeaker.Target => target.Instance,
                _ => throw new ArgumentOutOfRangeException()
            };

            var speakerController = block.speaker switch
            {
                DialogueActionBlock.DialogueSpeaker.Source => source,
                DialogueActionBlock.DialogueSpeaker.Target => target,
                _ => throw new ArgumentOutOfRangeException()
            };

            var dialogue = new Dialogue(unitInstance, block.text, onComplete);
            dialogueReference.StartDialogue(speakerController, dialogue);
        }
    }
}