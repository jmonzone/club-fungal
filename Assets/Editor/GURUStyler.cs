using UnityEditor;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public static class GURUStyler
{
    private static readonly Type[] serviceTypes = { typeof(GameService), typeof(UnitInstanceService), typeof(UnitControllerService), typeof(PartyInstanceService), typeof(PartyControllerService) };
    private static Dictionary<Type, GURUService> cachedServices = new Dictionary<Type, GURUService>();

    private class ButtonInfo
    {
        public string label;
        public Action action;
        public Func<bool> condition;
    }

    private static T LoadAsset<T>(string typeName) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeName);
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        return null;
    }

    private static void EnsureAssetsLoaded()
    {
        foreach (var type in serviceTypes)
        {
            if (!cachedServices.ContainsKey(type) || cachedServices[type] == null)
            {
                cachedServices[type] = LoadAsset<GURUService>(type.Name);
            }
        }
    }

    public static void DrawGuruSection(System.Action contentDrawer, string helpText = "", UnityEngine.Object target = null, string componentName = null)
    {
        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.65f, 0.25f, 0.7f, 0.4f);
        GUIStyle paddedHelpBox = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(15, 15, 15, 15)
        };
        EditorGUILayout.BeginVertical(paddedHelpBox);
        GUI.backgroundColor = Color.white;

        GUIStyle logoStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
        };
        string resolvedName = componentName ?? (target != null ? target.GetType().Name : null);
        string headerText = resolvedName != null ? $"GURU - {resolvedName}" : "GURU";
        EditorGUILayout.LabelField(headerText, logoStyle);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (!string.IsNullOrEmpty(helpText))
        {
            EditorGUILayout.HelpBox(helpText, MessageType.None);
        }
        EditorGUILayout.Space(15);

        contentDrawer();

        // Separator before buttons
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Ensure assets are loaded
        EnsureAssetsLoaded();

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical();
        GUIStyle linkStyle = new GUIStyle(EditorStyles.linkLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };

        var buttons = new List<ButtonInfo>();

        // Add buttons for each service
        foreach (var kvp in cachedServices)
        {
            string label = $"Open {kvp.Key.Name} Asset";
            Action action = () => AssetDatabase.OpenAsset(kvp.Value);
            Func<bool> condition = () => kvp.Value != null && Selection.activeObject != kvp.Value;
            buttons.Add(new ButtonInfo { label = label, action = action, condition = condition });
        }

        // Add reset button for GameService
        if (cachedServices.ContainsKey(typeof(GameService)) && cachedServices[typeof(GameService)] != null)
        {
            buttons.Add(new ButtonInfo { label = "Reset JSON File", action = () => { if (cachedServices[typeof(GameService)] is GameService gs) gs.ResetJsonFile(); }, condition = () => true });
        }

        // Add open JSON file button
        buttons.Add(new ButtonInfo { label = "Open JSON File", action = () => Process.Start(LocalData.GetSaveDataPath()), condition = () => true });

        foreach (var b in buttons)
        {
            if (b.condition())
            {
                if (GUILayout.Button(b.label, linkStyle))
                {
                    b.action();
                }
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        EditorGUILayout.EndVertical();
    }
}