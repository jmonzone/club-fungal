using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitInstanceService))]
public class UnitInstanceServiceEditor : GURUServiceEditor
{

    protected override void OnEditorEnable()
    {
        // UnityEngine.Debug.Log("UnitInstanceServiceEditor OnEnable - Auto-populating lists if empty");
        // Cache the populated lists on enable
        AutoPopulateList<UnitInstance>("initialUnits", "t:UnitInstance");
        AutoPopulateList<Unit>("unitCollection", "t:Unit");
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
        if (asset is Unit unit)
        {
            return !string.IsNullOrEmpty(unit.Name);
        }
        return true;
    }

    private UnitControllerService FindUnitControllerService()
    {
        var guids = AssetDatabase.FindAssets("t:UnitControllerService");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<UnitControllerService>(path);
        }
        return null;
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

        // Display units in a responsive layout
        List<UnitInstance> toRemove = new List<UnitInstance>();
        var controllerService = FindUnitControllerService();
        UnitListDrawer.DrawList(service.Units, unit =>
        {
            var item = UnitListDrawer.CreateBaseDrawerItem(unit, service.PartyInstanceService, true, (u) => service.Units.Contains(u) ? new Color(0.6f, 0.7f, 1.0f) : Color.white, true, (u) =>
            {
                if (service.PartyInstanceService.PartyInstances.Any(p => p.Id == u.Id))
                {
                    service.PartyInstanceService.RemoveUnitInstanceFromParty(u);
                }
                else
                {
                    service.PartyInstanceService.AddUnitInstanceToParty(u);
                }
            }, null);
            UnitListDrawer.AddViewButton(item, controllerService?.GetController(unit));
            if (!service.Units.Contains(unit))
            {
                item.Buttons.Add(("X", () => toRemove.Add(unit), () => true));
            }
            return item;
        });
        // Remove after iteration
        foreach (var unit in toRemove)
        {
            service.Units.Remove(unit);
            service.SaveData();
            EditorUtility.SetDirty(service);
        }

        EditorGUILayout.Space();

        // Buttons at the bottom
        GUIStyle leftAlignedButton = new GUIStyle(GUI.skin.button);
        leftAlignedButton.alignment = TextAnchor.MiddleLeft;
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(10);
        DrawIconButton("🆕", "Generate A New Unit", () => { service.CreateUnit(unit => unit is FungalUnit); EditorUtility.SetDirty(service); }, leftAlignedButton);
        EditorGUILayout.Space(10);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
    }

    protected override string GetHelpText()
    {
        return "Manage and inspect all units in the service. Initial units can be edited, runtime units can be removed.";
    }
}