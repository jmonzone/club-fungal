using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(DialogueInteraction))]
public class DialogueInteractionEditor : Editor
{
    private SerializedProperty dialogueListProperty;
    private ReorderableList dialogueList;

    private void OnEnable()
    {
        var stepsProperty = serializedObject.FindProperty("steps");
        dialogueList = new ReorderableList(serializedObject, stepsProperty, true, true, true, true);
        dialogueList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Interaction Steps");
        dialogueList.drawElementCallback = DrawStepElement;
        dialogueList.elementHeightCallback = GetStepElementHeight;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueReference"));

        dialogueList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawStepElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var element = dialogueList.serializedProperty.GetArrayElementAtIndex(index);
        var typeProp = element.FindPropertyRelative("type");
        var dialogueDataProp = element.FindPropertyRelative("dialogueData");
        var partyProp = element.FindPropertyRelative("party");

        // Type dropdown
        Rect typeRect = new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

        var typeIndex = typeProp.enumValueIndex;
        if (typeIndex == 0) // Dialogue
        {
            // Show dialogue data
            DrawDialogueData(rect, dialogueDataProp);
        }
        else if (typeIndex == 1) // JoinParty
        {
            // Show party field
            Rect partyRect = new Rect(rect.x + 105, rect.y, rect.width - 105, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(partyRect, partyProp, GUIContent.none);
        }
    }

    private void DrawDialogueData(Rect rect, SerializedProperty dialogueDataProp)
    {
        var speakerProp = dialogueDataProp.FindPropertyRelative("speaker");
        var unitInstanceProp = dialogueDataProp.FindPropertyRelative("unitInstance");
        var textProp = dialogueDataProp.FindPropertyRelative("text");

        // Display area
        Rect displayRect = new Rect(rect.x + 105, rect.y, rect.width - 105 - 85, EditorGUIUtility.singleLineHeight);
        var speakerIndex = speakerProp.enumValueIndex;
        if (speakerIndex == 2) // Specific
        {
            // Show icon and unit field
            Rect iconRect = new Rect(displayRect.x, displayRect.y, 32, displayRect.height);
            Sprite icon = null;
            if (unitInstanceProp.objectReferenceValue is UnitInstance unitInstance && unitInstance.Data != null)
            {
                icon = unitInstance.Data.Sprite;
            }
            if (icon != null)
            {
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.clear;
                EditorGUI.DrawTextureTransparent(iconRect, icon.texture, ScaleMode.ScaleToFit);
                GUI.backgroundColor = oldColor;
            }
            else
            {
                EditorGUI.LabelField(iconRect, "No Icon", EditorStyles.miniLabel);
            }

            Rect unitRect = new Rect(displayRect.x + 35, displayRect.y, displayRect.width - 35, displayRect.height);
            EditorGUI.PropertyField(unitRect, unitInstanceProp, GUIContent.none);
        }
        else
        {
            // Show label
            string label = speakerIndex == 0 ? "Source Unit" : "Target Unit";
            EditorGUI.LabelField(displayRect, label, EditorStyles.boldLabel);
        }

        // Speaker dropdown (last column)
        Rect speakerRect = new Rect(rect.x + rect.width - 80, rect.y, 80, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(speakerRect, speakerProp, GUIContent.none);

        // Text field
        Rect textRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, EditorGUIUtility.singleLineHeight * 2);
        textProp.stringValue = EditorGUI.TextArea(textRect, textProp.stringValue);
    }

    private float GetStepElementHeight(int index)
    {
        var element = dialogueList.serializedProperty.GetArrayElementAtIndex(index);
        var typeProp = element.FindPropertyRelative("type");
        var typeIndex = typeProp.enumValueIndex;
        if (typeIndex == 0) // Dialogue
        {
            return EditorGUIUtility.singleLineHeight * 3 + 4; // Dialogue height
        }
        else
        {
            return EditorGUIUtility.singleLineHeight + 4; // Simple height
        }
    }
}