using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PartyService))]
public class PartyServiceEditor : Editor
{
    private void OnEnable()
    {
        // Update PartyWidgetUI when the editor opens (in edit mode)
        if (!Application.isPlaying)
        {
            UpdateAllPartyWidgetUI();
        }
    }

    public override void OnInspectorGUI()
    {
        PartyService service = (PartyService)target;

        // Reinitialize party from JSON in edit mode
        if (!Application.isPlaying)
        {
            service.Initialize();
        }

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
                    onRemove: (unit) =>
                    {
                        EditorUtility.SetDirty(service);
                        UpdateAllPartyWidgetUI();
                    },
                    canRemoveFunc: (unit) => true, // Allow removing any
                    showPartyStatus: false // Already in party
                );
            }

            // Add a button to reinitialize party from JSON
            if (GUILayout.Button("Reinitialize Party from JSON"))
            {
                service.Initialize();
                UpdateAllPartyWidgetUI();
            }
        }, "Manage the current party instances.", service);
    }

    private void UpdateAllPartyWidgetUI()
    {
        var widgets = FindObjectsByType<PartyWidgetUI>(FindObjectsSortMode.None);
        foreach (var widget in widgets)
        {
            widget.UpdatePartyList();
        }
    }
}