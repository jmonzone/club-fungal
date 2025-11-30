using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueAction))]
public class DialogueActionDrawer : InteractionActionDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var speakerProp = property.FindPropertyRelative("speaker");
        var unitInstanceProp = property.FindPropertyRelative("unitInstance");
        var textProp = property.FindPropertyRelative("text");

        // Type label
        Rect typeRect = new Rect(position.x, position.y, 80, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(typeRect, "Dialogue", EditorStyles.boldLabel);

        // Display area
        Rect displayRect = new Rect(position.x + 85, position.y, position.width - 85 - 85, EditorGUIUtility.singleLineHeight);
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
        Rect speakerRect = new Rect(position.x + position.width - 80, position.y, 80, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(speakerRect, speakerProp, GUIContent.none);

        // Text field
        Rect textRect = new Rect(position.x + 85, position.y + EditorGUIUtility.singleLineHeight + 2, position.width - 85, EditorGUIUtility.singleLineHeight * 2);
        textProp.stringValue = EditorGUI.TextArea(textRect, textProp.stringValue);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3 + 4;
    }
}