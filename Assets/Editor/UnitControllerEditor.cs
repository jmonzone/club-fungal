using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitController), true, isFallback = true)]
public class UnitControllerEditor : RepositionableEditor
{
    private UnitInteraction selectedInteraction;

    protected override string Description =>
        "Displays the current behaviour of the unit and handles repositioning.";

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

        if (controller.CurrentInteraction != null)
        {
            EditorGUILayout.LabelField("Current Interaction", controller.CurrentInteraction.GetType().Name);
        }
        else
        {
            EditorGUILayout.LabelField("Current Interaction", "None");
        }

        selectedInteraction = (UnitInteraction)EditorGUILayout.ObjectField("Interaction to Add", selectedInteraction, typeof(UnitInteraction), false);
        if (GUILayout.Button("Add Moment") && selectedInteraction != null)
        {
            controller.AddMoment(selectedInteraction);
            selectedInteraction = null;
        }

        if (controller.Instance.OriginalInstance != null)
        {
            if (GUILayout.Button("Add to Original") && selectedInteraction != null)
            {
                var moment = new UnitMoment(selectedInteraction, false);
                controller.Instance.OriginalInstance.Moments.Add(moment);
                selectedInteraction = null;
                GURUInitializer.InitializeSystems();
            }
        }
    }
}