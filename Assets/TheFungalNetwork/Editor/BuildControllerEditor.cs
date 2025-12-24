using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildController), true)]
public class BuildControllerEditor : RepositionableEditor
{
    protected override void DrawContent()
    {
        BuildController controller = (BuildController)target;

        // Add specific drawing for BuildController if needed
        EditorGUILayout.LabelField("Build Controller", "Repositionable");
    }

    protected override string Description =>
        "Handles repositioning for BuildController.";
}