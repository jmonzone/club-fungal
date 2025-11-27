using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomManager roomManager = (RoomManager)target;

        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.65f, 0.25f, 0.7f, 0.4f);
        GUIStyle paddedHelpBox = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(15, 15, 15, 15)
        };
        EditorGUILayout.BeginVertical(paddedHelpBox);
        GUI.backgroundColor = Color.white;

        GUIStyle logoStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
        };
        EditorGUILayout.LabelField("ROOM MANAGER TOOLS", logoStyle);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Ceiling Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        if (GUILayout.Button("Show All Ceilings", GUILayout.Height(30)))
        {
            SetAllCeilings(roomManager, true);
        }

        GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f, 1f);
        if (GUILayout.Button("Hide All Ceilings", GUILayout.Height(30)))
        {
            SetAllCeilings(roomManager, false);
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void SetAllCeilings(RoomManager roomManager, bool visible)
    {
        RoomController[] rooms = roomManager.GetComponentsInChildren<RoomController>();

        foreach (var room in rooms)
        {
            SerializedObject roomObj = new SerializedObject(room);
            SerializedProperty ceilingProp = roomObj.FindProperty("ceilingObject");

            if (ceilingProp != null && ceilingProp.objectReferenceValue != null)
            {
                GameObject ceiling = ceilingProp.objectReferenceValue as GameObject;
                Undo.RecordObject(ceiling, visible ? "Show Ceiling" : "Hide Ceiling");
                ceiling.SetActive(visible);
                EditorUtility.SetDirty(ceiling);
            }
        }

        Debug.Log($"{(visible ? "Showed" : "Hid")} ceilings for {rooms.Length} room(s)");
    }
}
