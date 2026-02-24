using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

/// <summary>
/// Adds context menu to create ScriptableObject assets from script files.
/// Right-click any ScriptableObject-derived script in Project window -> "Create Asset from Script"
/// </summary>
public static class ScriptableObjectCreator
{
    [MenuItem("Assets/Create Asset from Script", true)]
    private static bool ValidateCreateAssetFromScript()
    {
        // Only show menu item if selected file is a ScriptableObject-derived script
        var script = Selection.activeObject as MonoScript;
        if (script == null) return false;

        var scriptClass = script.GetClass();
        if (scriptClass == null) return false;

        return scriptClass.IsSubclassOf(typeof(ScriptableObject)) && !scriptClass.IsAbstract;
    }

    [MenuItem("Assets/Create Asset from Script", false, 80)]
    private static void CreateAssetFromScript()
    {
        var script = Selection.activeObject as MonoScript;
        if (script == null) return;

        var scriptClass = script.GetClass();
        if (scriptClass == null || !scriptClass.IsSubclassOf(typeof(ScriptableObject)))
        {
            Debug.LogError("Selected script is not a ScriptableObject");
            return;
        }

        if (scriptClass.IsAbstract)
        {
            Debug.LogError("Cannot create instance of abstract ScriptableObject");
            return;
        }

        // Create instance
        var instance = ScriptableObject.CreateInstance(scriptClass);
        if (instance == null)
        {
            Debug.LogError($"Failed to create instance of {scriptClass.Name}");
            return;
        }

        // Determine save path (same folder as the script)
        string scriptPath = AssetDatabase.GetAssetPath(script);
        string scriptDirectory = Path.GetDirectoryName(scriptPath);
        string assetName = $"New {scriptClass.Name}.asset";
        string assetPath = Path.Combine(scriptDirectory, assetName);

        // Make path unique if file already exists
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        // Create and save asset
        AssetDatabase.CreateAsset(instance, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Select the newly created asset
        EditorGUIUtility.PingObject(instance);
        Selection.activeObject = instance;

        Debug.Log($"Created {scriptClass.Name} asset at {assetPath}");
    }
}
