using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(UnitControllerService), true)]
public class UnitControllerServiceEditor : GURUServiceEditor
{
    protected override void DrawContent()
    {
        UnitControllerService service = (UnitControllerService)target;

        EditorGUILayout.LabelField("Unit Controllers in Scene:", EditorStyles.boldLabel);

        UnitListDrawer.DrawList(service.Controllers.Select(controller => controller.Instance));
    }

    protected override string GetHelpText()
    {
        return "This section displays all Unit Controllers currently present in the scene, including inactive ones.";
    }
}