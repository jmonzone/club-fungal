using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PartyControllerService), true)]
public class PartyControllerServiceEditor : GURUServiceEditor
{

    protected override void DrawContent()
    {
        Debug.Log("Drawing PartyControllerServiceEditor content.");
        PartyControllerService service = (PartyControllerService)target;
        UnitListDrawer.DrawList(service.PartyControllers.Select(controller => controller.Instance));
    }

    protected override string GetHelpText()
    {
        return "This section displays all Party Controllers currently present in the scene, including inactive ones.";
    }
}