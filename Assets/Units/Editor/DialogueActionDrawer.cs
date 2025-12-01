using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueAction))]
public class DialogueActionDrawer : InteractionActionDrawer
{
    protected override void DrawCustom(Rect rect, SerializedProperty property)
    {
        var blocksProp = property.FindPropertyRelative("blocks");

        float currentY = rect.y;
        for (int i = 0; i < blocksProp.arraySize; i++)
        {
            var blockProp = blocksProp.GetArrayElementAtIndex(i);
            var speakerProp = blockProp.FindPropertyRelative("speaker");
            var textProp = blockProp.FindPropertyRelative("text");

            // Label for speaker
            Rect labelRect = new Rect(rect.x, currentY, rect.width - 105, EditorGUIUtility.singleLineHeight);
            string labelText = speakerProp.enumValueIndex == 0 ? "Source Unit" : "Target Unit";
            EditorGUI.LabelField(labelRect, labelText, EditorStyles.boldLabel);

            // Speaker dropdown
            Rect speakerRect = new Rect(rect.x + rect.width - 105, currentY, 80, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(speakerRect, speakerProp, GUIContent.none);

            // Remove button
            Rect removeRect = new Rect(rect.x + rect.width - 25, currentY, 25, EditorGUIUtility.singleLineHeight);
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUI.Button(removeRect, "X"))
            {
                blocksProp.DeleteArrayElementAtIndex(i);
                break; // to avoid index issues
            }
            GUI.backgroundColor = oldColor;

            // Text field
            string text = textProp.stringValue;
            int lineCount = string.IsNullOrEmpty(text) ? 1 : text.Split('\n').Length;
            float textHeight = EditorGUIUtility.singleLineHeight * Mathf.Max(2, lineCount);
            Rect textRect = new Rect(rect.x, currentY + EditorGUIUtility.singleLineHeight + 2, rect.width, textHeight);
            textProp.stringValue = EditorGUI.TextArea(textRect, textProp.stringValue);

            currentY += EditorGUIUtility.singleLineHeight + 2 + textHeight + 5;
        }

        // Add button
        if (GUI.Button(new Rect(rect.x + rect.width / 2 - 25, currentY, 50, EditorGUIUtility.singleLineHeight), "Add"))
        {
            blocksProp.InsertArrayElementAtIndex(blocksProp.arraySize);
            var newBlock = blocksProp.GetArrayElementAtIndex(blocksProp.arraySize - 1);
            newBlock.FindPropertyRelative("speaker").enumValueIndex = 0;
            newBlock.FindPropertyRelative("text").stringValue = "";
        }
    }

    protected override float GetCustomHeight(SerializedProperty property)
    {
        var blocksProp = property.FindPropertyRelative("blocks");
        float height = 0;
        for (int i = 0; i < blocksProp.arraySize; i++)
        {
            var blockProp = blocksProp.GetArrayElementAtIndex(i);
            var textProp = blockProp.FindPropertyRelative("text");
            string text = textProp.stringValue;
            int lineCount = string.IsNullOrEmpty(text) ? 1 : text.Split('\n').Length;
            float textHeight = EditorGUIUtility.singleLineHeight * Mathf.Max(2, lineCount);
            height += EditorGUIUtility.singleLineHeight + 2 + textHeight + 5;
        }
        height += EditorGUIUtility.singleLineHeight + 5; // add button
        return height;
    }
}