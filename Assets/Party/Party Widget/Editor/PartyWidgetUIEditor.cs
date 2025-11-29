using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PartyWidgetUI))]
public class PartyWidgetUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PartyWidgetUI ui = (PartyWidgetUI)target;

        if (GUILayout.Button("Update Party List"))
        {
            ui.UpdatePartyList();
        }
    }
}