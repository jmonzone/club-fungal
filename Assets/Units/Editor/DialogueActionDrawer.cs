using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueAction))]
public class DialogueActionDrawer : InteractionActionDrawer
{
    protected override void DrawCustom(Rect rect, SerializedProperty property)
    {
        var speakerProp = property.FindPropertyRelative("speaker");
        var unitInstanceProp = property.FindPropertyRelative("unitInstance");
        var textProp = property.FindPropertyRelative("text");
        var isFirstProp = property.FindPropertyRelative("isFirst");

        // Display area
        Rect displayRect = new Rect(rect.x, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight);
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
            string labelText = speakerIndex == 0 ? "Source Unit" : "Target Unit";
            EditorGUI.LabelField(displayRect, labelText, EditorStyles.boldLabel);
        }

        // Speaker dropdown
        Rect speakerRect = new Rect(rect.x + rect.width - 80, rect.y, 60, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(speakerRect, speakerProp, GUIContent.none);

        // IsFirst toggle
        Rect isFirstRect = new Rect(rect.x + rect.width - 20, rect.y, 20, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(isFirstRect, isFirstProp, GUIContent.none);

        // Text field
        string text = textProp.stringValue;
        int lineCount = string.IsNullOrEmpty(text) ? 1 : text.Split('\n').Length;
        float textHeight = EditorGUIUtility.singleLineHeight * Mathf.Max(2, lineCount);
        Rect textRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, textHeight);
        textProp.stringValue = EditorGUI.TextArea(textRect, textProp.stringValue);
    }

    protected override float GetCustomHeight(SerializedProperty property)
    {
        var textProp = property.FindPropertyRelative("text");
        string text = textProp.stringValue;
        int lineCount = string.IsNullOrEmpty(text) ? 1 : text.Split('\n').Length;
        float textHeight = EditorGUIUtility.singleLineHeight * Mathf.Max(2, lineCount);
        return EditorGUIUtility.singleLineHeight + 2 + textHeight;
    }
}