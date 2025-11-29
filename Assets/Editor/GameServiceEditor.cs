using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameService))]
public class GameServiceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GURUStyler.DrawGuruSection(() =>
        {
            GameService gameService = (GameService)target;

            if (GUILayout.Button("Reset JSON File"))
            {
                gameService.ResetJsonFile();
            }
        }, "Use this button to reset the JSON data and re-initialize all game systems.");
    }
}