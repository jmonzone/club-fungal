using System.Linq;
using UnityEditor;
using UnityEngine;

public static class UnitTypeUpdater
{
    [MenuItem("Tools/Update Unit Types")]
    public static void UpdateUnitTypes()
    {
        var fungalScript = Resources.FindObjectsOfTypeAll<MonoScript>().FirstOrDefault(s => s.GetClass() == typeof(FungalUnit));
        if (fungalScript == null)
        {
            Debug.LogError("FungalUnit script not found.");
            return;
        }
        var guids = AssetDatabase.FindAssets("t:Unit");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var unit = AssetDatabase.LoadAssetAtPath<Unit>(path);
            if (unit != null && !(unit is FungalUnit))
            {
                if (!unit.Name.Contains("Player", System.StringComparison.OrdinalIgnoreCase) && !unit.Name.Contains("Tree", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Change to FungalUnit
                    var so = new SerializedObject(unit);
                    so.FindProperty("m_Script").objectReferenceValue = fungalScript;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(unit);
                }
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Unit types updated to FungalUnit.");
    }
}