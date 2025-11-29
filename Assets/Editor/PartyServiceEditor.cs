using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PartyService))]
public class PartyServiceEditor : Editor
{
    private int selectedUnitIndex = 0;

    public override void OnInspectorGUI()
    {
        PartyService service = (PartyService)target;

        // Draw the default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Display party
        GURUStyler.DrawGuruSection(() =>
        {
            EditorGUILayout.LabelField("Party Instances:", EditorStyles.boldLabel);

            if (service.PartyInstances.Count == 0)
            {
                EditorGUILayout.HelpBox("No units in party.", MessageType.Info);
            }
            else
            {
                UnitInstanceListDrawer.DrawList(
                    service.PartyInstances,
                    service,
                    onRemove: (unit) =>
                    {
                        service.RemoveUnitInstanceFromParty(unit);
                        EditorUtility.SetDirty(service);
                    },
                    canRemoveFunc: (unit) => true, // Allow removing any
                    showPartyStatus: false // Already in party
                );
            }

            // Add a button to reinitialize party from JSON
            if (GUILayout.Button("Reinitialize Party from JSON"))
            {
                service.Initialize();
            }
        }, "Manage the current party instances.", service);
    }
}