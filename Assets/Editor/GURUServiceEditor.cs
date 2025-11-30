using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GURUService), true)]
public class GURUServiceEditor : Editor
{
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

    protected void OnEnable()
    {
        // Debug.Log("GURUServiceEditor OnEnable");
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        GameService gs = LoadAsset<GameService>("GameService");
        if (gs != null) gs.InitializeSystems();
        OnEditorEnable();
    }

    protected virtual void OnEditorEnable() { }

    protected void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        OnEditorDisable();
    }

    protected virtual void OnEditorDisable() { }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        Debug.Log($"Play mode state changed: {state}");
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Exiting play mode - Initializing GameService systems from GURUServiceEditor");
            GameService gs = LoadAsset<GameService>("GameService");
            if (gs != null) gs.InitializeSystems();
        }
    }

    public override void OnInspectorGUI()
    {
        GURUService service = (GURUService)target;

        DrawDefaultInspector();

        GURUStyler.DrawGuruSection(() => DrawContent(), GetHelpText(), service);
    }

    protected virtual void DrawContent()
    {
        Debug.Log("Drawing GURU Service Editor Content");
    }

    protected virtual string GetHelpText()
    {
        return "";
    }
}