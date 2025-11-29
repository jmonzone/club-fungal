using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public static class UnitInstanceListDrawer
{
    public static void DrawList(
        List<UnitInstance> units,
        PartyService partyService = null,
        Action<UnitInstance> onToggleParty = null,
        Action<UnitInstance> onRemove = null,
        Func<UnitInstance, bool> canRemoveFunc = null,
        Func<UnitInstance, string> toggleButtonTextFunc = null,
        Func<UnitInstance, Color> backgroundColorFunc = null,
        bool showPartyStatus = true)
    {
        List<UnitInstance> toRemove = new List<UnitInstance>();
        foreach (var unit in units)
        {
            if (unit == null)
            {
                EditorGUILayout.LabelField("Null unit in list", EditorStyles.boldLabel);
                continue;
            }
            EditorGUILayout.BeginHorizontal();
            Color originalBG = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColorFunc != null ? backgroundColorFunc(unit) : Color.white;
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
            if (showPartyStatus)
            {
                if (partyService != null)
                {
                    if (partyService.PartyInstances.Contains(unit))
                    {
                        EditorGUILayout.LabelField("🎉 In Party", jobStyle);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("PartyService not provided, cannot determine party status.", MessageType.Warning);
                }
            }
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
            string buttonText = toggleButtonTextFunc != null ? toggleButtonTextFunc(unit) : (partyService != null ? (partyService.PartyInstances.Contains(unit) ? "🎉" : "P") : "?");
            if (onToggleParty != null && GUILayout.Button(buttonText, GUILayout.Width(30), GUILayout.Height(20)))
            {
                onToggleParty(unit);
            }
            bool canRemove = canRemoveFunc == null || canRemoveFunc(unit);
            if (canRemove && onRemove != null)
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
            onRemove(unit);
        }
    }
}