using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

[CustomPropertyDrawer(typeof(InteractionAction), useForChildren: true)]
public class InteractionActionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var action = property.managedReferenceValue as InteractionAction;
        if (action != null)
        {
            string displayName = action.DisplayName;
            displayName = Regex.Replace(displayName, "([a-z])([A-Z])", "$1 $2");

            float y = position.y;
            Rect labelRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, displayName, EditorStyles.boldLabel);
            y += EditorGUIUtility.singleLineHeight + 2;

            var iterator = property.Copy();
            bool hasChildren = iterator.NextVisible(true);
            if (hasChildren)
            {
                do
                {
                    float height = EditorGUI.GetPropertyHeight(iterator);
                    Rect rect = new Rect(position.x, y, position.width, height);
                    EditorGUI.PropertyField(rect, iterator);
                    y += height;
                } while (iterator.NextVisible(false));
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + 2; // label + spacing

        var iterator = property.Copy();
        bool hasChildren = iterator.NextVisible(true);
        if (hasChildren)
        {
            do
            {
                height += EditorGUI.GetPropertyHeight(iterator);
            } while (iterator.NextVisible(false));
        }

        return height;
    }
}