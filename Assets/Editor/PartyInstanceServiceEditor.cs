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

        UnitListDrawer.DrawList(service.PartyInstances);
    }

    protected override string GetHelpText()
    {
        return "Manage the current party instances.";
    }
}