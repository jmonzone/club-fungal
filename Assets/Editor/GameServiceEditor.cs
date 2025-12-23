using UnityEngine;
using UnityEditor;
using System.Linq;

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

        var unitsInScene = gameService.UnitControllerService.Controllers;
        EditorGUILayout.LabelField("Units in Scene", EditorStyles.boldLabel);
        // UnitListDrawer.DrawList(controllers: unitsInScene);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        var otherUnits = gameService.UnitInstanceService.Instances
            .Where(instance => !unitsInScene.Any(controller => controller.Instance == instance));

        EditorGUILayout.LabelField("Other Units", EditorStyles.boldLabel);
        if (otherUnits.Count() == 0)
        {
            EditorGUILayout.HelpBox("All units are currently in the scene.", MessageType.Info);
        }
        else
        {
            // UnitListDrawer.DrawList(instances: otherUnits);
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.LabelField("Game Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Game"))
        {
            gameService.ResetData();
        }

        if (GUILayout.Button("View Local Data"))
        {
            System.Diagnostics.Process.Start(LocalData.GetSaveDataPath());
        }

    }

    protected override string Description => "Manage the core Game Service settings and operations.";
}