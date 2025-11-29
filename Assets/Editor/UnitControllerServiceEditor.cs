using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitControllerService), true)]
public class UnitControllerServiceEditor : GURUServiceEditor
{
    protected override void DrawContent()
    {
        UnitControllerService service = (UnitControllerService)target;

        EditorGUILayout.LabelField("Unit Controllers in Scene:", EditorStyles.boldLabel);

        UnitListDrawer.DrawList(service.UnitControllers, controller =>
        {
            var item = UnitListDrawer.CreateBaseDrawerItem(controller.Instance, service.UnitInstanceService.PartyInstanceService, true, null, false, null, null);
            if (controller.Instance == null)
            {
                item.DisplayName = controller.gameObject.name;
            }
            if (controller.Instance != null)
            {
                item.Buttons.Add(("View Instance Data", () => PopupInspector.Show(controller.Instance), () => true));
            }
            UnitListDrawer.AddViewButton(item, controller);
            return item;
        });
    }

    protected override string GetHelpText()
    {
        return "This section displays all Unit Controllers currently present in the scene, including inactive ones.";
    }
}