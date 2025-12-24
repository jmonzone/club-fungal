using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "DialogueInteraction", menuName = "Club Fungal/Interactions/Dialogue Interaction")]
public class DialogueInteraction : UnitInteraction
{
    [HideInInspector]
    [SerializeReference] private List<InteractionAction> actions;

    private UnitController source;
    private UnitController target;
    private int currentActionIndex = 0;

    public override void StartInteraction(UnitController source, UnitController target, UnityAction onComplete)
    {
        this.source = source;
        this.target = target;

        currentActionIndex = 0;

        foreach (var action in actions)
        {
            action.Initialize();
        }

        ExecuteNext(onComplete);
    }

    private void ExecuteNext(UnityAction onComplete)
    {
        if (currentActionIndex < actions.Count)
        {
            actions[currentActionIndex].Execute(source, target, () =>
            {
                currentActionIndex++;
                ExecuteNext(onComplete);
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }
}

[Serializable]
public abstract class InteractionAction
{
    public virtual void Initialize()
    {

    }

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

    public abstract void Execute(UnitController source, UnitController target, UnityAction onComplete);
}