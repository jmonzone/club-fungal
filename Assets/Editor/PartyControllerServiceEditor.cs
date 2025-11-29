using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PartyControllerService))]
public class PartyControllerServiceEditor : Editor
{
    private SerializedProperty partyControllersProp;

    private void OnEnable()
    {
        partyControllersProp = serializedObject.FindProperty("partyControllers");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        PartyControllerService service = (PartyControllerService)target;
        service.Initialize();
        GURUStyler.DrawGuruSection(() =>
        {
            if (GUILayout.Button("Clear Party Controllers"))
            {
                Undo.RecordObject(service, "Clear Party Controllers");
                partyControllersProp.ClearArray();
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Log Party Controllers Count"))
            {
                Debug.Log($"Party Controllers Count: {service.PartyControllers.Count}");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Party Controllers:", EditorStyles.boldLabel);

            if (service.PartyControllers.Count == 0)
            {
                EditorGUILayout.HelpBox("No party controllers.", MessageType.Info);
            }
            else
            {
                ControllerListDrawer.DrawList(service.PartyControllers.ToArray());
            }
        }, target: service);
    }
}