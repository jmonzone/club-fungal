using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InviteFriendAction))]
public class InviteFriendActionDrawer : InteractionActionDrawer
{
    protected override void DrawCustom(Rect rect, SerializedProperty property)
    {
        var unitControllerServiceProp = property.FindPropertyRelative("unitControllerService");

        // Unit Controller Service field
        Rect serviceRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(serviceRect, unitControllerServiceProp);
    }

    protected override float GetCustomHeight(SerializedProperty property)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}