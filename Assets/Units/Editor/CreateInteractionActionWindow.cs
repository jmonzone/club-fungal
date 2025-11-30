using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateInteractionActionWindow : EditorWindow
{
    private string actionName = "";

    public static void ShowWindow()
    {
        GetWindow<CreateInteractionActionWindow>("Create New Interaction Action");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Create New Interaction Action", EditorStyles.boldLabel);
        actionName = EditorGUILayout.TextField("Action Name", actionName);
        if (GUILayout.Button("Create"))
        {
            if (!string.IsNullOrEmpty(actionName))
            {
                CreateScript(actionName);
                Close();
            }
        }
        if (GUILayout.Button("Close"))
        {
            Close();
        }
    }

    private void CreateScript(string name)
    {
        string scriptContent = $@"
using UnityEngine;
using UnityEngine.Events;

public class {name}Action : InteractionAction
{{
    public override void Execute(UnitController source, UnitController target, DialogueReference dialogueReference, UnitControllerService unitControllerService, PartyInstanceService partyInstanceService, PartyControllerService partyControllerService, UnityAction onComplete)
    {{
        // Implement your action logic here
        onComplete?.Invoke();
    }}
}}
";
        string path = "Assets/Units/Interactions/" + name + "Action.cs";
        File.WriteAllText(path, scriptContent);
        AssetDatabase.Refresh();
    }
}