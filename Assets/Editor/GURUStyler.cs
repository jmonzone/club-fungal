using UnityEditor;
using UnityEngine;
using System.Diagnostics;

public static class GURUStyler
{
    private static GameService cachedGameService;
    private static ScriptableObject cachedUnitInstanceService;
    private static ScriptableObject cachedUnitControllerService;
    private static ScriptableObject cachedPartyInstanceService;

    private static void EnsureAssetsLoaded()
    {
        if (cachedGameService == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:GameService");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                cachedGameService = AssetDatabase.LoadAssetAtPath<GameService>(path);
            }
        }

        if (cachedUnitInstanceService == null)
        {
            string[] unitGuids = AssetDatabase.FindAssets("t:UnitInstanceService");
            if (unitGuids.Length > 0)
            {
                string unitPath = AssetDatabase.GUIDToAssetPath(unitGuids[0]);
                cachedUnitInstanceService = AssetDatabase.LoadAssetAtPath<ScriptableObject>(unitPath);
            }
        }

        if (cachedUnitControllerService == null)
        {
            string[] controllerGuids = AssetDatabase.FindAssets("t:UnitControllerService");
            if (controllerGuids.Length > 0)
            {
                string controllerPath = AssetDatabase.GUIDToAssetPath(controllerGuids[0]);
                cachedUnitControllerService = AssetDatabase.LoadAssetAtPath<ScriptableObject>(controllerPath);
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

        // GameService
        if (cachedGameService != null && Selection.activeObject != cachedGameService)
        {
            if (GUILayout.Button("Open GameService Asset", linkStyle))
            {
                AssetDatabase.OpenAsset(cachedGameService);
            }
        }

        // UnitInstanceService
        if (cachedUnitInstanceService != null && Selection.activeObject != cachedUnitInstanceService)
        {
            if (GUILayout.Button("Open UnitInstanceService Asset", linkStyle))
            {
                AssetDatabase.OpenAsset(cachedUnitInstanceService);
            }
        }

        // UnitControllerService
        if (cachedUnitControllerService != null && Selection.activeObject != cachedUnitControllerService)
        {
            if (GUILayout.Button("Open UnitControllerService Asset", linkStyle))
            {
                AssetDatabase.OpenAsset(cachedUnitControllerService);
            }
        }

        // Reset JSON File
        if (cachedGameService != null)
        {
            if (GUILayout.Button("Reset JSON File", linkStyle))
            {
                cachedGameService.ResetJsonFile();
            }
        }

        // Open JSON File
        if (GUILayout.Button("Open JSON File", linkStyle))
        {
            Process.Start(LocalData.GetSaveDataPath());
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        EditorGUILayout.EndVertical();
    }
}