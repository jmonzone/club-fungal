using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitSpecies))]
public class UnitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UnitSpecies unit = (UnitSpecies)target;

        // Draw the default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Guru styled section for conversion
        GURUStyler.DrawGuruSection(() =>
        {
            if (unit is FungalUnit)
            {
                EditorGUILayout.LabelField("This is already a FungalUnit.");
            }
            else
            {
                if (GUILayout.Button("Convert to FungalUnit", GUILayout.Height(30)))
                {
                    ConvertToFungalUnit(unit);
                }
            }
        }, "Convert this Unit to a FungalUnit, preserving all data and setting default column mapping.");
    }

    private void ConvertToFungalUnit(UnitSpecies unit)
    {
        string path = AssetDatabase.GetAssetPath(unit);

        // Create new FungalUnit
        FungalUnit fungalUnit = CreateInstance<FungalUnit>();

        // Manually copy the serialized fields using reflection
        var unitType = typeof(UnitSpecies);
        var fields = unitType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.IsDefined(typeof(SerializeField), true) || field.Name == "name")
            {
                var value = field.GetValue(unit);
                field.SetValue(fungalUnit, value);
            }
        }

        // Replace the asset
        AssetDatabase.CreateAsset(fungalUnit, path);
        AssetDatabase.Refresh();

        // Select the new asset
        Selection.activeObject = fungalUnit;

        Debug.Log($"Converted {unit.name} to FungalUnit at {path}");
    }
}