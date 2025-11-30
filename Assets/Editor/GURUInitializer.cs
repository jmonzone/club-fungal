using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class GURUInitializer
{
    static GURUInitializer()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Debug.Log("GURUServiceInitializer detected play mode state change: " + state);
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            InitializeSystems();
        }
    }

    public static void InitializeSystems()
    {
        var gs = LoadAsset<GameService>("GameService");
        if (gs != null) gs.InitializeSystems();
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
}
