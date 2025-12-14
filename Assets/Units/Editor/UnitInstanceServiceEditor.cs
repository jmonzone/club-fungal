using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitInstanceService))]
public class UnitInstanceServiceEditor : GURUEditor
{
    protected override void OnEditorEnable()
    {
        // UnityEngine.Debug.Log("UnitInstanceServiceEditor OnEnable - Auto-populating lists if empty");
        // Cache the populated lists on enable
        AutoPopulateList<UnitInstance>("initialUnits", "t:UnitInstance");
        AutoPopulateList<UnitSpecies>("unitCollection", "t:Unit");
        AutoPopulateList<UnitInteraction>("interactionCollection", "t:UnitInteraction");
    }

    private List<T> AutoPopulateList<T>(string fieldName, string assetType, bool force = false) where T : Object
    {
        var field = typeof(UnitInstanceService).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<T>)field.GetValue(target);
        list.Clear();
        var guids = AssetDatabase.FindAssets(assetType);
        foreach (var guid in guids)
        {
            // Debug.Log($"Found asset GUID: {guid} for type {typeof(T).Name}");
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null && IsValidAsset(asset))
            {
                list.Add(asset);
            }
        }
        EditorUtility.SetDirty(target);
        return list;
    }

    private bool IsValidAsset<T>(T asset) where T : Object
    {
        if (asset is UnitInstance unitInstance)
        {
            return unitInstance.Data != null && !string.IsNullOrEmpty(unitInstance.DisplayName);
        }
        if (asset is UnitSpecies unit)
        {
            return !string.IsNullOrEmpty(unit.Name);
        }
        return true;
    }

    private void DrawIconButton(string emoji, string text, System.Action action, GUIStyle style = null)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(emoji, GUILayout.Width(20));
        if (GUILayout.Button(text, style ?? GUI.skin.button))
        {
            action();
        }
        EditorGUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        UnitInstanceService service = (UnitInstanceService)target;

        // Check for null references
        var localData = service.LocalData;
        if (localData == null)
        {
            EditorGUILayout.HelpBox("LocalData is not assigned. Please assign it in the inspector.", MessageType.Error);
        }

        // Call base to draw the GURU section
        base.OnInspectorGUI();
    }

    protected override void DrawContent()
    {
        UnitInstanceService service = (UnitInstanceService)target;

        UnitListDrawer.DrawList(service.Instances);
        EditorGUILayout.Space(10);
        DrawIconButton("🆕", "Generate A New Unit", () => { service.CreateUnit(unit => unit is QuirkySeriesUnitSpecies); EditorUtility.SetDirty(service); });
    }

    protected override string Description =>
        "Manage all Unit Instances available in the game. You can also generate new units using the button below.";
}