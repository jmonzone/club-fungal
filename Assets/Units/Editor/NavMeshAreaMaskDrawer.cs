#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom property drawer for NavMeshAreaMask attribute.
/// Displays a flags-style dropdown for multiple NavMesh area selection.
/// </summary>
[CustomPropertyDrawer(typeof(NavMeshAreaMaskAttribute))]
public class NavMeshAreaMaskDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, label.text, "Use NavMeshAreaMask with int fields only");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // Get all NavMesh area names
        string[] areaNames = UnityEngine.AI.NavMesh.GetAreaNames();

        // Create display names with area indices
        string[] displayNames = new string[areaNames.Length];
        for (int i = 0; i < areaNames.Length; i++)
        {
            displayNames[i] = $"{i}: {areaNames[i]}";
        }

        // Use MaskField for flags-style multi-select dropdown
        int newMask = EditorGUI.MaskField(position, label, property.intValue, displayNames);

        if (newMask != property.intValue)
        {
            property.intValue = newMask;
        }

        EditorGUI.EndProperty();
    }
}
#endif
