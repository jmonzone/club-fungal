using UnityEngine;

public class Response : ScriptableObject
{
    [SerializeField][TextArea] private string text;
    [SerializeField] private float xp;
    [SerializeField] private float relationship;
    [SerializeField] private Dialogue next;

    public string Text => text;
    public float XP => xp;
    public float Relationship => relationship;
    public Dialogue Next => next;
}
