using UnityEditor;
using UnityEngine;

public static class GURUStyler
{
    public static void DrawGuruSection(System.Action contentDrawer, string helpText = "")
    {
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
        EditorGUILayout.LabelField("GURU", logoStyle);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (!string.IsNullOrEmpty(helpText))
        {
            EditorGUILayout.HelpBox(helpText, MessageType.None);
        }
        EditorGUILayout.Space(15);

        contentDrawer();

        EditorGUILayout.Space(15);

        EditorGUILayout.EndVertical();
    }
}