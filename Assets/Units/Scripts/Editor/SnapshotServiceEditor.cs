using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SnapshotService))]
public class SnapshotServiceEditor : GURUEditor
{
    protected override string Description =>
        "Snapshot Service: Save and load unit positions.";
    protected override void DrawContent()
    {
        SnapshotService service = (SnapshotService)target;

        if (GUILayout.Button("Save Snapshot to JSON"))
        {
            service.SaveSnapshot();
        }

        if (GUILayout.Button("Load Snapshot from JSON"))
        {
            service.LoadSnapshot();
        }

        if (GUILayout.Button("Save Snapshot As Asset"))
        {
            string path = EditorUtility.SaveFilePanel("Save Snapshot As Asset", "Assets/", "SnapshotInstance", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                // Make relative to Assets
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                SnapshotInstance instance = CreateInstance<SnapshotInstance>();
                foreach (var unit in service.UnitControllerService.Controllers)
                {
                    if (unit.Instance != null)
                    {
                        instance.snapshots.Add(new UnitSnapshot { id = unit.Instance.Id, position = unit.transform.position });
                    }
                }
                AssetDatabase.CreateAsset(instance, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}