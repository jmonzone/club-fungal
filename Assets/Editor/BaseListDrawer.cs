using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public abstract class UnitListDrawer
{
    public static void DrawList<T>(IEnumerable<T> items, Func<T, DrawerItem> toDrawerItem)
    {
        var drawerItems = items.Select(toDrawerItem).ToList();
        drawerItems.Sort((a, b) => string.Compare(a.Id, b.Id));
        DrawListInternal(drawerItems);
    }

    private static void DrawListInternal(List<DrawerItem> items)
    {
        foreach (var item in items)
        {
            EditorGUILayout.BeginHorizontal();
            Color originalBG = GUI.backgroundColor;
            GUI.backgroundColor = item.BackgroundColor;
            Color originalColor = GUI.color;
            GUI.color = Color.white;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // Top: icon and info horizontal
            EditorGUILayout.BeginHorizontal();
            // Left side: icon
            EditorGUILayout.BeginVertical();
            // Draw icon
            if (item.Icon != null)
            {
                GUIContent content = new GUIContent(item.Icon);
                GUILayout.Label(content, GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                GUILayout.Label("No Icon", GUILayout.Width(32), GUILayout.Height(32));
            }
            EditorGUILayout.EndVertical();
            // Right side: name and job
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(item.DisplayName);
            EditorGUILayout.Space(-2);
            GUIStyle jobStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Italic, normal = { textColor = new Color(0.5f, 0.5f, 0.5f) } };
            EditorGUILayout.LabelField(item.Job, jobStyle);
            if (item.IsInParty)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("🎉 In Party", jobStyle);
                GUI.color = Color.white;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            // ... button
            if (GUILayout.Button("...", GUILayout.Width(20), GUILayout.Height(20)))
            {
                ShowMenu(item);
            }
            // X button outside the help box, below
            if (item.Buttons.Any(b => b.text == "X"))
            {
                var removeButton = item.Buttons.First(b => b.text == "X");
                if (removeButton.condition())
                {
                    Color originalBG2 = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        removeButton.action();
                    }
                    GUI.backgroundColor = originalBG2;
                }
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = originalBG;
            GUI.color = originalColor;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }
    }

    public static DrawerItem CreateBaseDrawerItem(UnitInstance unitInstance, PartyInstanceService partyInstanceService, bool showPartyStatus, Func<UnitInstance, Color> backgroundColorFunc = null, bool addPopupButton = false, Action<UnitInstance> onToggleParty = null, Func<UnitInstance, string> toggleButtonTextFunc = null)
    {
        var item = new DrawerItem
        {
            Id = unitInstance?.Id ?? "",
            Icon = unitInstance?.Data?.Sprite?.texture,
            DisplayName = unitInstance?.DisplayName ?? "No Instance",
            Job = unitInstance?.Job?.Id.ToUpper() ?? "No Job",
            IsInParty = showPartyStatus && partyInstanceService != null && unitInstance != null && partyInstanceService.PartyInstances.Any(p => p.Id == unitInstance.Id),
            BackgroundColor = backgroundColorFunc != null && unitInstance != null ? backgroundColorFunc(unitInstance) : Color.white,
        };
        if (addPopupButton && unitInstance != null)
        {
            item.Buttons.Add(("View Data", () => PopupInspector.Show(unitInstance), () => true));
        }
        if (onToggleParty != null && unitInstance != null)
        {
            string buttonText = toggleButtonTextFunc?.Invoke(unitInstance) ?? (item.IsInParty ? "Remove from Party" : "Add to Party");
            item.Buttons.Add((buttonText, () => onToggleParty(unitInstance), () => true));
        }
        return item;
    }

    public static void AddViewButton(DrawerItem item, UnitController controller)
    {
        if (controller != null)
        {
            item.Buttons.Add(("View GameObject", () => { Selection.activeObject = controller.gameObject; EditorGUIUtility.PingObject(controller.gameObject); }, () => true));
        }
    }

    private static void ShowMenu(DrawerItem item)
    {
        GenericMenu menu = new GenericMenu();
        foreach (var (text, action, condition) in item.Buttons.Where(b => b.text != "X"))
        {
            if (condition())
            {
                menu.AddItem(new GUIContent(text), false, () => action());
            }
        }
        menu.ShowAsContext();
    }
}

public class DrawerItem
{
    public Texture2D Icon;
    public string Id;
    public string DisplayName;
    public string Job;
    public bool IsInParty;
    public Color BackgroundColor = Color.white;
    public List<(string text, Action action, Func<bool> condition)> Buttons = new List<(string, Action, Func<bool>)>();
}