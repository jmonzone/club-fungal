using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(UnitControllerService), true)]
public class UnitControllerServiceEditor : GURUEditor
{
    protected override void DrawContent()
    {
        UnitControllerService service = (UnitControllerService)target;
        UnitListDrawer.DrawList(service.Controllers.Select(controller => controller.Instance));
    }

    protected override string Description =>
        "This section displays all Unit Controllers currently present in the scene, including inactive ones.";
}