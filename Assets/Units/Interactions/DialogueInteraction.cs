using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "DialogueInteraction", menuName = "Club Fungal/Interactions/Dialogue Interaction")]
public class DialogueInteraction : UnitInteraction
{
    [SerializeField] private DialogueReference dialogueReference;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private PartyInstanceService partyInstanceService;
    [SerializeField] private PartyControllerService partyControllerService;

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

        dialogueReference.StartDialogueInteraction(new List<UnitController> { source, target });

        currentActionIndex = 0;
        ExecuteNext();
    }

    private void ExecuteNext()
    {
        if (currentActionIndex < actions.Count)
        {
            actions[currentActionIndex].Execute(source, target, dialogueReference, unitControllerService, partyInstanceService, partyControllerService, () =>
            {
                currentActionIndex++;
                ExecuteNext();
            });
        }
        else
        {
            dialogueReference.CompleteDialogue();
        }
    }
}

[Serializable]
public abstract class InteractionAction
{
    public abstract void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete);
}

[Serializable]
public class DialogueAction : InteractionAction
{
    public enum DialogueSpeaker
    {
        Source,
        Target,
        Specific,
    }
    [SerializeField] private DialogueSpeaker speaker;
    [SerializeField][TextArea] private string text;
    [SerializeField] private UnitInstance unitInstance;

    public DialogueSpeaker Speaker => speaker;
    public UnitInstance UnitInstance => unitInstance;

    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete)
    {
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

[Serializable]
public class JoinPartyAction : InteractionAction
{
    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete)
    {
        partyControllerService.AddToParty(target);
        Debug.Log("Joined party!");
        onComplete();
    }
}