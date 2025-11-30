using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

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

            // Handle double-click to open script
            HandleDoubleClickToOpenScript(labelRect, action);

            Rect customRect = new Rect(position.x, y, position.width, position.height - (y - position.y));
            DrawCustom(customRect, property);
        }
    }

    protected virtual void DrawCustom(Rect rect, SerializedProperty property)
    {
        var iterator = property.Copy();
        bool hasChildren = iterator.NextVisible(true);
        if (hasChildren)
        {
            float y = rect.y;
            do
            {
                float height = EditorGUI.GetPropertyHeight(iterator);
                Rect propRect = new Rect(rect.x, y, rect.width, height);
                EditorGUI.PropertyField(propRect, iterator);
                y += height;
            } while (iterator.NextVisible(false));
        }
    }

    protected static void HandleDoubleClickToOpenScript(Rect rect, InteractionAction action)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.clickCount == 2 && rect.Contains(Event.current.mousePosition))
        {
            MonoScript[] scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            var script = scripts.FirstOrDefault(s => s.GetClass() == action.GetType());
            if (script != null)
            {
                AssetDatabase.OpenAsset(script);
            }
            Event.current.Use();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + 2; // label + spacing
        height += GetCustomHeight(property);
        return height;
    }

    protected virtual float GetCustomHeight(SerializedProperty property)
    {
        float height = 0;
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