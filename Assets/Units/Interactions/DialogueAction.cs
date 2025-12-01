using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public enum DialogueSpeaker
{
    Source,
    Target,
}

[Serializable]
public class DialogueActionBlock
{
    [SerializeField] public DialogueSpeaker speaker;
    [SerializeField][TextArea] public string text;
}

[Serializable]
public class DialogueAction : InteractionAction
{
    public override string DisplayName => "Dialogue";

    [SerializeField] private DialogueReference dialogueReference;
    [SerializeField] private List<DialogueActionBlock> blocks = new();

    private int currentIndex = 0;

    public override void Execute(UnitController source, UnitController target, UnityAction onComplete)
    {
        if (currentIndex == 0)
        {
            source.Dialogue.StartDialogue(target);
            target.Dialogue.StartDialogue(source);

            dialogueReference.StartDialogueInteraction(new List<UnitController> { source, target });
        }

        var block = blocks[currentIndex];
        var unitInstance = block.speaker switch
        {
            DialogueSpeaker.Source => source.Instance,
            DialogueSpeaker.Target => target.Instance,
            _ => throw new ArgumentOutOfRangeException()
        };

        var speakerController = block.speaker switch
        {
            DialogueSpeaker.Source => source,
            DialogueSpeaker.Target => target,
            _ => throw new ArgumentOutOfRangeException()
        };

        var dialogue = new Dialogue(block.text);

        dialogueReference.StartDialogue(speakerController, dialogue, onContinue: () =>
        {
            currentIndex++;
            if (currentIndex < blocks.Count)
            {
                Execute(source, target, onComplete);
            }
            else
            {
                dialogueReference.CompleteDialogue();
                onComplete?.Invoke();
                currentIndex = 0; // reset for next execution
            }
        });
    }
}