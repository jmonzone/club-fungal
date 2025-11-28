using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class Dialogue
{
    [SerializeField] private UnitInstance unitInstance;
    [SerializeField][TextArea] private string text;
    [SerializeField] private Dialogue next; // next line in the chain
    public UnitInstance UnitInstance => unitInstance;
    public string Text => text;
    public Dialogue Next => next;

    // Single-line constructor
    public Dialogue(UnitInstance unitInstance, string text)
    {
        this.unitInstance = unitInstance;
        this.text = text;
    }

    // List-based constructor that chains dialogues
    public Dialogue(List<Dialogue> dialogueList)
    {
        if (dialogueList == null || dialogueList.Count == 0)
            throw new ArgumentException("Dialogue list cannot be null or empty");

        unitInstance = dialogueList[0].unitInstance;
        text = dialogueList[0].text;

        if (dialogueList.Count > 1)
        {
            // Recursively chain the remaining dialogues
            next = new Dialogue(dialogueList.GetRange(1, dialogueList.Count - 1));
        }
    }

    // Sequence constructor with unit instances
    public Dialogue(List<(UnitInstance unit, string text)> lines)
    {
        if (lines == null || lines.Count == 0)
            throw new ArgumentException("Lines cannot be null or empty");

        unitInstance = lines[0].unit;
        text = lines[0].text;

        if (lines.Count > 1)
        {
            // Recursively chain the remaining lines
            next = new Dialogue(lines.GetRange(1, lines.Count - 1));
        }
    }

    public void SetNext(Dialogue nextDialogue)
    {
        next = nextDialogue;
    }

    public static Dialogue BuildChain(JArray lineGroup, UnitInstance unitInstance)
    {
        Dialogue first = null;
        Dialogue previous = null;

        foreach (var lineToken in lineGroup)
        {
            var dialogue = new Dialogue(unitInstance, lineToken.ToString());
            if (first == null)
                first = dialogue;

            previous?.SetNext(dialogue);
            previous = dialogue;
        }

        return first;
    }
}