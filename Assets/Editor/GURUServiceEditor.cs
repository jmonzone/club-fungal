using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class GURUServiceInitializer
{
    static GURUServiceInitializer()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        Debug.Log("GURUServiceInitializer detected play mode state change: " + state);
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            var gs = LoadAsset<GameService>("GameService");
            if (gs != null) gs.InitializeSystems();
        }
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

[CustomEditor(typeof(GURUService), true)]
public class GURUServiceEditor : Editor
{

    protected void OnEnable()
    {
        OnEditorEnable();
    }

    protected virtual void OnEditorEnable() { }

    protected void OnDisable()
    {
        OnEditorDisable();
    }

    protected virtual void OnEditorDisable() { }

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