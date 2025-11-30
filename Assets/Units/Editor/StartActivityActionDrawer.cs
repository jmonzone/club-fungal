using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StartActivityAction))]
public class StartActivityActionDrawer : InteractionActionDrawer
{
    protected override void DrawCustom(Rect rect, SerializedProperty property)
    {
        var activityRefProp = property.FindPropertyRelative("activityReference");
        Rect activityRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(activityRect, activityRefProp, new GUIContent("Activity Reference"));
    }

    protected override float GetCustomHeight(SerializedProperty property)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}