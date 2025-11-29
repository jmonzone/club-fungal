using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PartyControllerService), true)]
public class PartyControllerServiceEditor : GURUServiceEditor
{
    private SerializedProperty partyControllersProp;

    private void OnEnable()
    {
        partyControllersProp = serializedObject.FindProperty("partyControllers");
    }

    protected override void DrawContent()
    {
        PartyControllerService service = (PartyControllerService)target;
        UnitListDrawer.DrawList(service.PartyControllers, controller =>
        {
            var item = UnitListDrawer.CreateBaseDrawerItem(controller.Instance, null, true, null, false, null, null);
            if (controller.Instance == null)
            {
                item.DisplayName = controller.gameObject.name;
            }
            item.Buttons.Add(("👁", () => { Selection.activeObject = controller.gameObject; EditorGUIUtility.PingObject(controller.gameObject); }, () => true));
            return item;
        });
    }

    protected override string GetHelpText()
    {
        return "This section displays all Party Controllers currently present in the scene, including inactive ones.";
    }
}