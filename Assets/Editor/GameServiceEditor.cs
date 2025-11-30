using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameService), true)]
public class GameServiceEditor : GURUEditor
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

    protected override string Description => "Manage the core Game Service settings and operations.";
}