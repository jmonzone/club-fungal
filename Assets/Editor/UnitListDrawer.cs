using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public abstract class UnitListDrawer
{
    public static void DrawList(IEnumerable<UnitInstance> items)
    {
        var drawerItems = items.Select(CreateBaseDrawerItem).ToList();
        drawerItems.Sort((a, b) => string.Compare(a.Id, b.Id));
        DrawListInternal(drawerItems);
    }

    private static void DrawListInternal(List<UnitDrawerItem> items)
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

    public static UnitDrawerItem CreateBaseDrawerItem(UnitInstance unitInstance)
    {
        var unitInstanceService = Resources.FindObjectsOfTypeAll<UnitInstanceService>().FirstOrDefault();
        var partyInstanceService = Resources.FindObjectsOfTypeAll<PartyInstanceService>().FirstOrDefault();
        var unitControllerService = Resources.FindObjectsOfTypeAll<UnitControllerService>().FirstOrDefault();

        var item = new UnitDrawerItem
        {
            Id = unitInstance?.Id ?? "",
            Icon = unitInstance?.Data?.Sprite?.texture,
            DisplayName = unitInstance?.DisplayName ?? "No Instance",
            Job = unitInstance?.Job?.Id.ToUpper() ?? "No Job",
            IsInParty = partyInstanceService.PartyInstances.Any(p => p.Id == unitInstance.Id)
        };

        var initialUnit = unitInstanceService.InitialUnits.Find(instance => instance.Id == unitInstance.Id);
        item.BackgroundColor = initialUnit != null ? new Color(0.5f, 0.6f, 1f) : Color.white;

        if (initialUnit)
        {
            item.Buttons.Add(("View Asset", () => { Selection.activeObject = unitInstance; EditorGUIUtility.PingObject(unitInstance); }, () => true));
        }

        item.Buttons.Add(("View Instance", () => PopupInspector.Show(unitInstance), () => true));

        var controller = unitControllerService.Controllers.Find(c => c.Instance != null && c.Instance.Id == unitInstance.Id);
        if (controller != null)
        {
            item.Buttons.Add(("View GameObject", () => { Selection.activeObject = controller.gameObject; EditorGUIUtility.PingObject(controller.gameObject); }, () => true));
        }


        string buttonText = (item.IsInParty ? "Remove from Party" : "Add to Party");
        item.Buttons.Add((buttonText, (Action)(() =>
        {
            if (item.IsInParty)
                partyInstanceService.RemoveUnitInstanceFromParty(unitInstance);
            else
                partyInstanceService.AddUnitInstanceToParty(unitInstance);
        }), (Func<bool>)(() => true)));
        return item;
    }
    private static void ShowMenu(UnitDrawerItem item)
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

public class UnitDrawerItem
{
    public Texture2D Icon;
    public string Id;
    public string DisplayName;
    public string Job;
    public bool IsInParty;
    public Color BackgroundColor = Color.white;
    public List<(string text, Action action, Func<bool> condition)> Buttons = new List<(string, Action, Func<bool>)>();
}