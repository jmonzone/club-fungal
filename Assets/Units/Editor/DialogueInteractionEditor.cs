using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

[CustomEditor(typeof(DialogueInteraction))]
public class DialogueInteractionEditor : Editor
{
    private ReorderableList dialogueList;

    private void OnEnable()
    {
        var actionsProperty = serializedObject.FindProperty("actions");
        dialogueList = new ReorderableList(serializedObject, actionsProperty, true, true, false, false);
        dialogueList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Interaction Sequence");
        dialogueList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = dialogueList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element);
        };
        dialogueList.elementHeightCallback = (int index) =>
        {
            var element = dialogueList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element);
        };
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
        var actionTypes = TypeCache.GetTypesDerivedFrom<InteractionAction>();
        foreach (var type in actionTypes)
        {
            if (!type.IsAbstract && type != typeof(DialogueAction)) // Exclude DialogueAction as it's added via button
            {
                string name = type.Name.Replace("Action", "").Replace("Interaction", "");
                name = Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
                menu.AddItem(new GUIContent(name), false, () => AddAction((InteractionAction)Activator.CreateInstance(type)));
            }
        }
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Create New Interaction Action"), false, () => CreateInteractionActionWindow.ShowWindow());
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