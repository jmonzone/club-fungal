using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(JoinPartyAction))]
public class JoinPartyActionDrawer : InteractionActionDrawer
{
    protected override void DrawCustom(Rect rect, SerializedProperty property)
    {
        var partyControllerServiceProp = property.FindPropertyRelative("partyControllerService");

        // Party Controller Service field
        Rect serviceRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(serviceRect, partyControllerServiceProp);
    }

    protected override float GetCustomHeight(SerializedProperty property)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}