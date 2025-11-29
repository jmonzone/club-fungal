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

    public virtual void DrawUnitInstances(List<UnitInstance> units, PartyInstanceService partyInstanceService = null, Action<UnitInstance> onToggleParty = null, Action<UnitInstance> onRemove = null, Func<UnitInstance, bool> canRemoveFunc = null, Func<UnitInstance, string> toggleButtonTextFunc = null, Func<UnitInstance, Color> backgroundColorFunc = null, bool showPartyStatus = true)
    {
        if (showPartyStatus && partyInstanceService == null)
        {
            EditorGUILayout.HelpBox("Party Instance Service is not assigned. Party status will not be shown.", MessageType.Warning);
        }
        if (units.Count == 0)
        {
            EditorGUILayout.HelpBox("No unit instances found.", MessageType.Info);
            return;
        }
        List<UnitInstance> toRemove = new List<UnitInstance>();
        DrawList(units, unit =>
        {
            var item = CreateBaseDrawerItem(unit, partyInstanceService, showPartyStatus, backgroundColorFunc, true, onToggleParty, toggleButtonTextFunc);
            if (canRemoveFunc?.Invoke(unit) ?? true)
            {
                item.Buttons.Add(("X", () => toRemove.Add(unit), () => true));
            }
            return item;
        });
        // Remove after iteration
        foreach (var unit in toRemove)
        {
            onRemove?.Invoke(unit);
        }
    }

    public virtual void DrawUnitControllers(UnitController[] controllers, PartyInstanceService partyInstanceService = null, Action<UnitController> onToggleParty = null, Action<UnitController> onRemove = null, Func<UnitController, bool> canRemoveFunc = null, Func<UnitController, string> toggleButtonTextFunc = null, Func<UnitController, Color> backgroundColorFunc = null, bool showPartyStatus = true)
    {
        if (showPartyStatus && partyInstanceService == null)
        {
            EditorGUILayout.HelpBox("Party Instance Service is not assigned. Party status will not be shown.", MessageType.Warning);
        }
        if (controllers.Length == 0)
        {
            EditorGUILayout.HelpBox("No unit controllers found.", MessageType.Info);
            return;
        }
        DrawList(controllers, controller =>
        {
            var item = CreateBaseDrawerItem(controller.Instance, partyInstanceService, showPartyStatus, backgroundColorFunc != null ? (Func<UnitInstance, Color>)(u => backgroundColorFunc(controller)) : null, false, onToggleParty != null ? (Action<UnitInstance>)(u => onToggleParty(controller)) : null, toggleButtonTextFunc != null ? (Func<UnitInstance, string>)(u => toggleButtonTextFunc(controller)) : null);
            if (controller.Instance == null)
            {
                item.DisplayName = controller.gameObject.name;
            }
            if (canRemoveFunc?.Invoke(controller) ?? false)
            {
                item.Buttons.Add(("X", () => onRemove?.Invoke(controller), () => true));
            }
            AddViewButton(item, controller);
            return item;
        });
    }
}

public class DrawerItem
{
    public Texture2D Icon;
    public string DisplayName;
    public string Job;
    public bool IsInParty;
    public Color BackgroundColor = Color.white;
    public List<(string text, Action action, Func<bool> condition)> Buttons = new List<(string, Action, Func<bool>)>();
}