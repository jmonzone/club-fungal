using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PartyInstanceService), true)]
public class PartyInstanceServiceEditor : GURUServiceEditor
{
    private int selectedUnitIndex = 0;

    public override void OnInspectorGUI()
    {
        PartyInstanceService service = (PartyInstanceService)target;

        base.OnInspectorGUI();
    }

    protected override void DrawContent()
    {
        PartyInstanceService service = (PartyInstanceService)target;

        EditorGUILayout.LabelField("Party Instances:", EditorStyles.boldLabel);

        UnitListDrawer.DrawList(service.PartyInstances, unit =>
        {
            var item = UnitListDrawer.CreateBaseDrawerItem(unit, service, false, null, true, null, null);
            item.Buttons.Add(("X", () => { service.RemoveUnitInstanceFromParty(unit); EditorUtility.SetDirty(service); }, () => true));
            return item;
        });
    }

    protected override string GetHelpText()
    {
        return "Manage the current party instances.";
    }
}