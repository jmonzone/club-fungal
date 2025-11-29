using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;

[CustomEditor(typeof(UnitInstanceService))]
public class UnitInstanceServiceEditor : Editor
{
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

        // Auto populate initialUnits
        var initialUnits = AutoPopulateList<UnitInstance>("initialUnits", "t:UnitInstance");

        // Auto populate unitCollection
        var unitCollection = AutoPopulateList<Unit>("unitCollection", "t:Unit");

        // Draw the default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Display units
        GURUStyler.DrawGuruSection(() =>
        {
            var combined = new List<(UnitInstance unit, bool isInitial)>();
            var seenIds = new HashSet<string>();
            foreach (var u in initialUnits)
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
            UnitInstanceListDrawer.DrawList(
                unitsToDraw,
                onToggleParty: (unit) =>
                {
                    var localDataField = typeof(UnitInstanceService).GetField("localData", BindingFlags.NonPublic | BindingFlags.Instance);
                    var localData = (LocalData)localDataField.GetValue(service);

                    // Ensure localData is initialized
                    if (localData.JsonFile == null) localData.Initialize();

                    var partyArray = localData.JsonFile["party"] as JArray ?? new JArray();
                    if (unit.IsInParty)
                    {
                        // Remove from party
                        var itemToRemove = partyArray.FirstOrDefault(t => t.ToString() == unit.Id);
                        if (itemToRemove != null) partyArray.Remove(itemToRemove);
                    }
                    else
                    {
                        // Add to party
                        if (!partyArray.Any(t => t.ToString() == unit.Id))
                        {
                            partyArray.Add(unit.Id);
                        }
                    }
                    localData.JsonFile["party"] = partyArray;
                    localData.SaveData("party", partyArray);
                    unit.IsInParty = !unit.IsInParty;
                },
                onRemove: (unit) =>
                {
                    service.Units.Remove(unit);
                    service.SaveData();
                    EditorUtility.SetDirty(service);
                },
                canRemoveFunc: (unit) => !initialUnits.Contains(unit),
                backgroundColorFunc: (unit) => initialUnits.Contains(unit) ? new Color(0.6f, 0.7f, 1.0f) : Color.white
            );

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
            DrawIconButton("🔄", "Update Collections", () => { AutoPopulateList<UnitInstance>("initialUnits", "t:UnitInstance", true); AutoPopulateList<Unit>("unitCollection", "t:Unit", true); }, leftAlignedButton);
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }, "Manage and inspect all units in the service. Initial units can be edited, runtime units can be removed.", service);
    }
}