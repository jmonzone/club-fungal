using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitInstanceService))]
public class UnitInstanceServiceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UnitInstanceService service = (UnitInstanceService)target;

        // Draw the default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Display units
        EditorGUILayout.LabelField("Units", EditorStyles.boldLabel);
        List<UnitInstance> toRemove = new List<UnitInstance>();
        foreach (var unit in service.Units)
        {
            EditorGUILayout.BeginHorizontal();
            // Draw icon
            if (unit.Data != null && unit.Data.Sprite != null)
            {
                GUIContent content = new GUIContent(unit.Data.Sprite.texture);
                GUILayout.Label(content, GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                GUILayout.Label("No Icon", GUILayout.Width(32), GUILayout.Height(32));
            }
            // Name
            EditorGUILayout.LabelField(unit.Data != null ? unit.Data.Name : "Unknown", GUILayout.ExpandWidth(true));
            // Remove button
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                toRemove.Add(unit);
            }
            EditorGUILayout.EndHorizontal();
        }
        // Remove after iteration
        foreach (var unit in toRemove)
        {
            service.Units.Remove(unit);
            service.SaveData();
        }
        if (toRemove.Count > 0)
        {
            EditorUtility.SetDirty(service);
        }

        EditorGUILayout.Space();

        // Button to initialize from local data
        if (GUILayout.Button("Initialize from Local Data"))
        {
            service.Initialize();
            EditorUtility.SetDirty(service);
        }

        EditorGUILayout.Space();

        // Button to open JSON file
        if (GUILayout.Button("Open JSON File"))
        {
            string path = $"{Application.persistentDataPath}/data-editor.json";
            Process.Start(path);
        }
    }
}