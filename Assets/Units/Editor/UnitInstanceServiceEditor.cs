using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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
                if (asset != null)
                {
                    list.Add(asset);
                }
            }
            EditorUtility.SetDirty(target);
        }
        return list;
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

            // Display units in a responsive layout
            List<UnitInstance> toRemove = new List<UnitInstance>();
            foreach (var (unit, isInitial) in combined)
            {
                EditorGUILayout.BeginHorizontal();
                Color originalBG = GUI.backgroundColor;
                GUI.backgroundColor = isInitial ? new Color(0.6f, 0.7f, 1.0f) : Color.white;
                Color originalColor = GUI.color;
                GUI.color = Color.white;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                // Draw icon
                if (unit.Data != null && unit.Data.Sprite != null)
                {
                    GUIContent content = new GUIContent(unit.Data.Sprite.texture);
                    GUILayout.Label(content, GUILayout.Width(32), GUILayout.Height(32));
                }
                else
                {
                    GUILayout.Label("No Icon", GUILayout.Width(32), GUILayout.Height(32));
                }
                // Vertical for name and job
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(unit.DisplayName);
                EditorGUILayout.Space(-2); // Reduce space
                GUIStyle jobStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Italic, normal = { textColor = new Color(0.5f, 0.5f, 0.5f) } };
                EditorGUILayout.LabelField(unit.Job != null ? $"{unit.Job.Id.ToUpper()}" : "No Job", jobStyle);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = originalBG;
                GUI.color = originalColor;
                // Buttons next to the box
                EditorGUILayout.BeginVertical(GUILayout.Width(30));
                if (GUILayout.Button("...", GUILayout.Width(30), GUILayout.Height(20)))
                {
                    PopupInspector.Show(unit);
                }
                if (!isInitial)
                {
                    Color buttonColor = GUI.color;
                    GUI.color = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(20)))
                    {
                        toRemove.Add(unit);
                    }
                    GUI.color = buttonColor;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                // Space between cards
                EditorGUILayout.Space(5);
            }

            // Remove after iteration
            foreach (var unit in toRemove)
            {
                service.Units.Remove(unit);
                service.SaveData();
            }
            if (toRemove.Count > 0)
            {
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
            DrawIconButton("📂", "Open JSON File", () => { string path = $"{Application.persistentDataPath}/data-editor.json"; Process.Start(path); }, leftAlignedButton);
            EditorGUILayout.Space(5);
            DrawIconButton("🔄", "Reset To Default", () => { service.Reset(); EditorUtility.SetDirty(service); }, leftAlignedButton);
            EditorGUILayout.Space(5);
            DrawIconButton("🔄", "Update Collections", () => { AutoPopulateList<UnitInstance>("initialUnits", "t:UnitInstance", true); AutoPopulateList<Unit>("unitCollection", "t:Unit", true); }, leftAlignedButton);
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }, "Manage and inspect all units in the service. Initial units can be edited, runtime units can be removed.");
    }
}