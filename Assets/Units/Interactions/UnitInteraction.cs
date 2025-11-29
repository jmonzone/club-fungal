using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnitInteractionInstance
{
    [SerializeField] private UnitInteraction interaction;
    [SerializeField] private bool isComplete;

    public UnitInteraction Interaction => interaction;
    public bool IsComplete => isComplete;
    public event UnityAction OnInteractionComplete;

    public UnitInteractionInstance(UnitInteraction interaction, bool isComplete)
    {
        this.interaction = interaction;
        this.isComplete = isComplete;
    }

    public void StartInteraction(UnitController source, UnitController target)
    {
        interaction.StartInteraction(source, target, () =>
        {
            isComplete = true;
            OnInteractionComplete?.Invoke();
        });
    }
}

public abstract class UnitInteraction : ScriptableObject
{
    [SerializeField] private string id;
    public string ID => id;

    public virtual void StartInteraction(UnitController source, UnitController target, UnityAction onComplete)
    {
    }
}
