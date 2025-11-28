using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Dialogue
{
    [SerializeField] private UnitInstance unitInstance;
    [SerializeField][TextArea] private string text;

    private UnityAction onComplete;
    public string Text => text;

    // Single-line constructor
    public Dialogue(UnitInstance unitInstance, string text, UnityAction onComplete = null)
    {
        this.unitInstance = unitInstance;
        this.text = text;
        this.onComplete = onComplete;
    }

    public void Continue()
    {
        onComplete?.Invoke();
    }
}