using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GURUService), true)]
public class GURUEditor : Editor
{
    protected virtual string Description => "No description provided.";

    protected void OnEnable()
    {
        OnEditorEnable();
        GURUInitializer.InitializeSystems();
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
    }
}