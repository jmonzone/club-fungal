#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

/// <summary>
/// Custom property drawer for NavMeshArea attribute.
/// Displays a dropdown of all NavMesh areas instead of a raw integer field.
/// </summary>
[CustomPropertyDrawer(typeof(NavMeshAreaAttribute))]
public class NavMeshAreaDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, label.text, "Use NavMeshArea with int fields only");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // Get all NavMesh area names
        string[] areaNames = NavMesh.GetAreaNames();
        int currentAreaIndex = property.intValue;

        // Find the current selection in the list
        int selectedIndex = currentAreaIndex;
        if (currentAreaIndex < 0 || currentAreaIndex >= areaNames.Length)
        {
            selectedIndex = 0;
        }

        // Create display names with area indices
        string[] displayNames = new string[areaNames.Length];
        for (int i = 0; i < areaNames.Length; i++)
        {
            displayNames[i] = $"{i}: {areaNames[i]}";
        }

        // Show dropdown
        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, displayNames);

        if (newIndex != currentAreaIndex)
        {
            property.intValue = newIndex;
        }

        EditorGUI.EndProperty();
    }
}
#endif
