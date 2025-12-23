using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GURUService), true)]
public class GURUEditor : Editor
{
    protected virtual string Description => "No description provided.";

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
        // GURUStyler.DrawGuruSection(() => DrawContent(), Description, target);

        // EditorGUILayout.Space(10);

        DrawDefaultInspector();
    }

    protected virtual void DrawContent()
    {
    }
}