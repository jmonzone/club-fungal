using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GURUService), true)]
public class GURUServiceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GURUService service = (GURUService)target;

        DrawDefaultInspector();

        GURUStyler.DrawGuruSection(() => DrawContent(), GetHelpText(), service);
    }

    protected virtual void DrawContent()
    {
        Debug.Log("Drawing GURU Service Editor Content");
    }

    protected virtual string GetHelpText()
    {
        return "";
    }
}