using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitController), true)]
public class UnitControllerEditor : GURUEditor
{
    protected override void DrawContent()
    {
        UnitController controller = (UnitController)target;

        if (controller.CurrentBehaviour != null)
        {
            EditorGUILayout.LabelField("Current Behaviour", controller.CurrentBehaviour.GetType().Name);
        }
        else
        {
            EditorGUILayout.LabelField("Current Behaviour", "None");
        }
    }

    protected override string Description =>
        "Displays the current behaviour of the unit.";
}