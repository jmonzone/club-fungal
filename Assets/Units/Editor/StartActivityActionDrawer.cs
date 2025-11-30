using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StartActivityAction))]
public class StartActivityActionDrawer : InteractionActionDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var activityRefProp = property.FindPropertyRelative("activityReference");

        // Type label
        Rect typeRect = new Rect(position.x, position.y, 100, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(typeRect, "Start Activity", EditorStyles.boldLabel);

        // Activity reference field
        Rect activityRect = new Rect(position.x + 105, position.y, position.width - 105, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(activityRect, activityRefProp, new GUIContent("Activity Reference"));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}