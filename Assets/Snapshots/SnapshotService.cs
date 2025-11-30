using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SnapshotService", menuName = "Club Fungal/Snapshot Service")]
public class SnapshotService : GURUService
{
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private LocalData localData;
    [SerializeField] private SnapshotInstance defaultSnapshotInstance;

    public UnitControllerService UnitControllerService => unitControllerService;

    private const string SNAPSHOT_KEY = "snapshot";

    protected override void OnInitialize()
    {
        Application.quitting += SaveSnapshot;
        LoadSnapshot();
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();
        LoadSnapshot();
    }

    public void SaveSnapshot()
    {
        if (unitControllerService.Controllers.Count == 0) return;

        Debug.Log($"Saving snapshot of unit positions controllers. Total controllers: {unitControllerService.Controllers.Count}");
        var snapshotJson = new JArray();

        foreach (var unit in unitControllerService.Controllers)
        {
            if (unit.Instance != null)
            {
                var unitJson = new JObject
                {
                    ["id"] = unit.Instance.Id,
                    ["position"] = new JObject
                    {
                        ["x"] = unit.transform.position.x,
                        ["y"] = unit.transform.position.y,
                        ["z"] = unit.transform.position.z
                    }
                };
                snapshotJson.Add(unitJson);
            }
        }

        localData.SaveData(SNAPSHOT_KEY, snapshotJson);
    }

    public void LoadSnapshotFromAsset(SnapshotInstance instance)
    {
        Debug.Log("Loading snapshot from asset.");
        foreach (var snap in instance.snapshots)
        {
            var targetController = unitControllerService.Controllers.Find(controller => controller.Instance.Id == snap.id);
            if (targetController != null)
            {
                targetController.transform.position = snap.position;
            }
        }
    }

    public Dictionary<string, Vector3> LoadSnapshot()
    {
        Debug.Log("Loading snapshot of unit positions.");
        var positions = new Dictionary<string, Vector3>();

        if (localData.JsonFile.ContainsKey(SNAPSHOT_KEY))
        {
            var snapshotArray = localData.JsonFile[SNAPSHOT_KEY] as JArray;
            if (snapshotArray != null)
            {
                foreach (var item in snapshotArray)
                {
                    if (item is JObject obj)
                    {
                        string id = obj.Value<string>("id");
                        var targetController = unitControllerService.Controllers.Find(controller => controller.Instance.Id == id);
                        if (targetController == null) continue;

                        var posObj = obj["position"] as JObject;
                        if (posObj == null) continue;

                        float x = posObj.Value<float>("x");
                        float y = posObj.Value<float>("y");
                        float z = posObj.Value<float>("z");
                        positions[id] = new Vector3(x, y, z);
                        targetController.Teleport(positions[targetController.Instance.Id], targetController.transform.parent);
                    }
                }
            }
        }

        if (positions.Count == 0 && defaultSnapshotInstance != null)
        {
            Debug.Log("No snapshot found in JSON, loading default snapshot.");
            foreach (var snap in defaultSnapshotInstance.snapshots)
            {
                var targetController = unitControllerService.Controllers.Find(controller => controller.Instance.Id == snap.id);
                if (targetController != null)
                {
                    targetController.Teleport(snap.position, targetController.transform.parent);
                    positions[snap.id] = snap.position;
                }
            }

            SaveSnapshot();
        }

        return positions;
    }
}