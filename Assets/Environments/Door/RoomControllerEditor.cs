using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(RoomController))]
public class RoomControllerEditor : Editor
{
    private Texture textureToApply;
    private List<Texture> textureHistory = new List<Texture>();
    private int currentHistoryIndex = -1;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomController roomController = (RoomController)target;

        if (textureToApply == null)
            textureToApply = roomController.CurrentMaterial;

        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.3f);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("⚙️ EDITOR TOOLS", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        textureToApply = (Texture)EditorGUILayout.ObjectField("Current Texture", textureToApply, typeof(Texture), false);
        if (EditorGUI.EndChangeCheck() && textureToApply != null)
        {
            // Remove any forward history when a new texture is applied
            if (currentHistoryIndex < textureHistory.Count - 1)
            {
                textureHistory.RemoveRange(currentHistoryIndex + 1, textureHistory.Count - currentHistoryIndex - 1);
            }

            textureHistory.Add(textureToApply);
            currentHistoryIndex = textureHistory.Count - 1;
            roomController.ApplyTextureToAllWalls(textureToApply);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // Undo button
        EditorGUI.BeginDisabledGroup(currentHistoryIndex <= 0);
        if (GUILayout.Button("↶", GUILayout.Width(30), GUILayout.Height(30)))
        {
            if (currentHistoryIndex > 0)
            {
                currentHistoryIndex--;
                textureToApply = textureHistory[currentHistoryIndex];
                roomController.ApplyTextureToAllWalls(textureToApply);
            }
        }
        EditorGUI.EndDisabledGroup();

        // Redo button
        EditorGUI.BeginDisabledGroup(currentHistoryIndex >= textureHistory.Count - 1);
        if (GUILayout.Button("↷", GUILayout.Width(30), GUILayout.Height(30)))
        {
            if (currentHistoryIndex < textureHistory.Count - 1)
            {
                currentHistoryIndex++;
                textureToApply = textureHistory[currentHistoryIndex];
                roomController.ApplyTextureToAllWalls(textureToApply);
            }
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}
