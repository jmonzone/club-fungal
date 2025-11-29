using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(DialogueInteraction))]
public class DialogueInteractionEditor : Editor
{
    private ReorderableList dialogueList;

    private void OnEnable()
    {
        var actionsProperty = serializedObject.FindProperty("actions");
        dialogueList = new ReorderableList(serializedObject, actionsProperty, true, true, false, false);
        dialogueList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Interaction Actions");
        dialogueList.drawElementCallback = DrawActionElement;
        dialogueList.elementHeightCallback = GetActionElementHeight;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        GURUStyler.DrawGuruSection(() =>
        {
            dialogueList.DoLayoutList();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Dialogue"))
            {
                AddAction(new DialogueAction());
            }
            if (GUILayout.Button("Add Action"))
            {
                ShowAddActionMenu();
            }
            if (GUILayout.Button("Remove"))
            {
                RemoveLastAction();
            }
            EditorGUILayout.EndHorizontal();
        }, "Customize interaction actions for dialogue and other behaviors");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawActionElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var element = dialogueList.serializedProperty.GetArrayElementAtIndex(index);
        var action = element.managedReferenceValue;
        if (action is DialogueAction)
        {
            DrawDialogueAction(rect, element);
        }
        else if (action is JoinPartyAction)
        {
            DrawJoinPartyAction(rect, element);
        }
        else
        {
            EditorGUI.LabelField(rect, "Unknown action type");
        }
    }

    private void DrawDialogueAction(Rect rect, SerializedProperty element)
    {
        var speakerProp = element.FindPropertyRelative("speaker");
        var unitInstanceProp = element.FindPropertyRelative("unitInstance");
        var textProp = element.FindPropertyRelative("text");

        // Type label
        Rect typeRect = new Rect(rect.x, rect.y, 80, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(typeRect, "Dialogue", EditorStyles.boldLabel);

        // Display area
        Rect displayRect = new Rect(rect.x + 85, rect.y, rect.width - 85 - 85, EditorGUIUtility.singleLineHeight);
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

        // Speaker dropdown
        Rect speakerRect = new Rect(rect.x + rect.width - 80, rect.y, 80, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(speakerRect, speakerProp, GUIContent.none);

        // Text field
        Rect textRect = new Rect(rect.x + 85, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width - 85, EditorGUIUtility.singleLineHeight * 2);
        textProp.stringValue = EditorGUI.TextArea(textRect, textProp.stringValue);
    }

    private void DrawJoinPartyAction(Rect rect, SerializedProperty element)
    {
        // Type label
        Rect typeRect = new Rect(rect.x, rect.y, 80, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(typeRect, "Join Party", EditorStyles.boldLabel);
    }

    private void DrawDialogueData(Rect rect, SerializedProperty dialogueDataProp)
    {
        // This method is no longer used
    }

    private float GetActionElementHeight(int index)
    {
        var element = dialogueList.serializedProperty.GetArrayElementAtIndex(index);
        var action = element.managedReferenceValue;
        if (action is DialogueAction)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 4;
        }
        else if (action is JoinPartyAction)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }
        else
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }
    }

    private void AddAction(InteractionAction action)
    {
        var actionsProp = serializedObject.FindProperty("actions");
        actionsProp.InsertArrayElementAtIndex(actionsProp.arraySize);
        var newElement = actionsProp.GetArrayElementAtIndex(actionsProp.arraySize - 1);
        newElement.managedReferenceValue = action;
        serializedObject.ApplyModifiedProperties();
    }

    private void ShowAddActionMenu()
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Join Party"), false, () => AddAction(new JoinPartyAction()));
        // Add more actions here as needed
        menu.ShowAsContext();
    }

    private void RemoveLastAction()
    {
        var actionsProp = serializedObject.FindProperty("actions");
        if (actionsProp.arraySize > 0)
        {
            actionsProp.DeleteArrayElementAtIndex(actionsProp.arraySize - 1);
            serializedObject.ApplyModifiedProperties();
        }
    }
}