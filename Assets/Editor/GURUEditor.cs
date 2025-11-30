using UnityEditor;
using UnityEngine;

public class GURUEditor : Editor
{
    protected void OnEnable()
    {
        OnEditorEnable();
    }

    protected virtual void OnEditorEnable() { }

    protected void OnDisable()
    {
        OnEditorDisable();
    }

    protected virtual void OnEditorDisable() { }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GURUStyler.DrawGuruSection(() => DrawContent(), Description, target);
    }

    protected virtual void DrawContent()
    {
        Debug.Log("Drawing GURU Service Editor Content");
    }

    protected virtual string Description => "No description provided.";
}