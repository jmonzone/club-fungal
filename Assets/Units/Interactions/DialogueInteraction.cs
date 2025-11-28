using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueInteraction", menuName = "Club Fungal/Interactions/Dialogue Interaction")]
public class DialogueInteraction : UnitInteraction
{
    [SerializeField] private DialogueReference dialogueReference;

    [SerializeField] private List<InteractionStepData> steps;

    public override void StartInteraction(UnitController source, UnitController target)
    {
        source.Dialogue.StartDialogue(target);
        target.Dialogue.StartDialogue(source);
        target.Focus();

        foreach (var step in steps)
        {
            step.Execute(source, target, dialogueReference);
        }
    }
}

[Serializable]
public class InteractionStepData
{
    public enum StepType { Dialogue, JoinParty }

    [SerializeField] private StepType type;
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private PartyReference party; // for JoinParty

    public void Execute(UnitController source, UnitController target, DialogueReference dialogueReference)
    {
        switch (type)
        {
            case StepType.Dialogue:
                var unitInstance = dialogueData.Speaker switch
                {
                    DialogueData.DialogueSpeaker.Source => source.Instance,
                    DialogueData.DialogueSpeaker.Target => target.Instance,
                    _ => dialogueData.UnitInstance
                };
                var dialogue = new Dialogue(unitInstance, dialogueData.Text);
                dialogueReference.StartDialogue(dialogue);
                break;
            case StepType.JoinParty:
                // TODO: implement join party logic
                Debug.Log("Joining party: " + party);
                break;
        }
    }
}

[Serializable]
public class DialogueData
{
    public enum DialogueSpeaker { Source, Target, Specific }

    [SerializeField] private DialogueSpeaker speaker;
    [SerializeField] private UnitInstance unitInstance;
    [SerializeField][TextArea] private string text;

    public DialogueSpeaker Speaker => speaker;
    public UnitInstance UnitInstance => unitInstance;
    public string Text => text;
}
