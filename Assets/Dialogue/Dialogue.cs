using System;
using UnityEngine;

[Serializable]
public class Dialogue
{
    [SerializeField][TextArea] private string text;
    public string Text => text;

    public Dialogue(string text)
    {
        this.text = text;
    }
}