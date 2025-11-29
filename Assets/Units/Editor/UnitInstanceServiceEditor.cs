using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitInstanceService))]
public class UnitInstanceServiceEditor : GURUServiceEditor
{
    private List<UnitInstance> cachedInitialUnits;
    private List<Unit> cachedUnitCollection;

    private void OnEnable()
    {
        // Cache the populated lists on enable
        cachedInitialUnits = AutoPopulateList<UnitInstance>("initialUnits", "t:UnitInstance");
        cachedUnitCollection = AutoPopulateList<Unit>("unitCollection", "t:Unit");
    }
    private List<T> AutoPopulateList<T>(string fieldName, string assetType, bool force = false) where T : Object
    {
        var field = typeof(UnitInstanceService).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<T>)field.GetValue(target);
        if (list.Count == 0 || force)
        {
            list.Clear();
            var guids = AssetDatabase.FindAssets(assetType);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null && IsValidAsset(asset))
                {
                    list.Add(asset);
                }
            }
            EditorUtility.SetDirty(target);
        }
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

        EditorGUILayout.Space();

        // Display units
        var combined = new List<(UnitInstance unit, bool isInitial)>();
        var seenIds = new HashSet<string>();
        foreach (var u in cachedInitialUnits)
        {
            combined.Add((u, true));
            seenIds.Add(u.Id);
        }
        foreach (var u in service.Units)
        {
            if (!seenIds.Contains(u.Id))
            {
                combined.Add((u, false));
                seenIds.Add(u.Id);
            }
        }
        combined = combined.OrderBy(c => c.unit.Id).ToList();

        var unitsToDraw = combined.Select(c => c.unit).ToList();
        var ghostUnits = unitsToDraw.Where(u => u.Data == null || string.IsNullOrEmpty(u.DisplayName)).ToList();
        if (ghostUnits.Any())
        {
            EditorGUILayout.HelpBox($"Ghost units detected ({ghostUnits.Count}): {string.Join(", ", ghostUnits.Select(u => $"ID:{u.Id ?? "null"}"))}", MessageType.Error);
        }
        unitsToDraw = unitsToDraw.Where(u => u.Data != null && !string.IsNullOrEmpty(u.DisplayName)).ToList();

        // Display units in a responsive layout
        List<UnitInstance> toRemove = new List<UnitInstance>();
        UnitListDrawer.DrawList(unitsToDraw, unit =>
        {
            var item = UnitListDrawer.CreateBaseDrawerItem(unit, service.PartyInstanceService, true, (u) => cachedInitialUnits.Contains(u) ? new Color(0.6f, 0.7f, 1.0f) : Color.white, true, (u) =>
            {
                if (service.PartyInstanceService.PartyInstances.Any(p => p.Id == u.Id))
                {
                    service.PartyInstanceService.RemoveUnitInstanceFromParty(u);
                }
                else
                {
                    service.PartyInstanceService.AddUnitInstanceToParty(u);
                }
                u.IsInParty = !u.IsInParty;
            }, null);
            if (!cachedInitialUnits.Contains(unit))
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
        EditorGUILayout.Space(5);
        DrawIconButton("🔄", "Update Collections", () =>
        {
            cachedInitialUnits = AutoPopulateList<UnitInstance>("initialUnits", "t:UnitInstance", true);
            cachedUnitCollection = AutoPopulateList<Unit>("unitCollection", "t:Unit", true);
        }, leftAlignedButton);
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