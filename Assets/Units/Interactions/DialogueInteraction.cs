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

    public override void StartInteraction(UnitController source, UnitController target, UnityAction onComplete)
    {
        this.source = source;
        this.target = target;

        source.Dialogue.StartDialogue(target);
        target.Dialogue.StartDialogue(source);

        dialogueReference.StartDialogueInteraction(new List<UnitController> { source, target });

        currentActionIndex = 0;
        ExecuteNext(onComplete);
    }

    private void ExecuteNext(UnityAction onComplete)
    {
        if (currentActionIndex < actions.Count)
        {
            actions[currentActionIndex].Execute(source, target, dialogueReference, unitControllerService, partyInstanceService, partyControllerService, () =>
            {
                currentActionIndex++;
                ExecuteNext(onComplete);
            });
        }
        else
        {
            dialogueReference.CompleteDialogue();
            onComplete?.Invoke();
        }
    }
}

[Serializable]
public abstract class InteractionAction
{
    public virtual string DisplayName
    {
        get
        {
            string name = GetType().Name;
            if (name.EndsWith("Action")) name = name.Substring(0, name.Length - 6);
            if (name.EndsWith("Interaction")) name = name.Substring(0, name.Length - 11);
            return name;
        }
    }

    public abstract void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete);
}