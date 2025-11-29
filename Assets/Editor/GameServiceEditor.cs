using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameService), true)]
public class GameServiceEditor : GURUServiceEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    protected override void DrawContent()
    {
        GameService gameService = (GameService)target;

        if (GUILayout.Button("Reset JSON File"))
        {
            gameService.ResetJsonFile();
        }
    }

    protected override string GetHelpText()
    {
        return "Use this button to reset the JSON data and re-initialize all game systems.";
    }
}