using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateInteractionActionWindow : EditorWindow
{
    private string actionName = "NewInteractionAction";
    private string id = "";

    public static void ShowWindow()
    {
        var window = GetWindow<CreateInteractionActionWindow>("Create New Interaction Action");
        window.minSize = new Vector2(400, 150);
        window.maxSize = new Vector2(400, 150);
    }

    void OnGUI()
    {
        GUIContent createContent = new GUIContent("Create ✅");
        GUIContent closeContent = new GUIContent("Close ❌");

        EditorGUILayout.BeginVertical();
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Create New Interaction Action", EditorStyles.boldLabel);
        GUILayout.Space(10);
        actionName = EditorGUILayout.TextField("Action Name", actionName);
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(createContent))
        {
            if (!string.IsNullOrEmpty(actionName))
            {
                CreateScript(actionName, id);
                Close();
            }
        }
        if (GUILayout.Button(closeContent))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void CreateScript(string name, string id)
    {
        string scriptContent = $@"
using UnityEngine;
using UnityEngine.Events;

public class {name}Action : InteractionAction
{{
    public override void Execute(UnitController source, UnitController target, UnityAction onComplete)
    {{
        // Implement your action logic here
        onComplete?.Invoke();
    }}
}}
";
        string path = "Assets/Units/Interactions/" + name + "Action.cs";
        File.WriteAllText(path, scriptContent);
        AssetDatabase.Refresh();
        var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
        Selection.activeObject = script;
        AssetDatabase.OpenAsset(script);
    }
}