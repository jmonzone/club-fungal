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

    public UnitControllerService UnitControllerService => unitControllerService;

    private const string SNAPSHOT_KEY = "snapshot";

    protected override void OnInitialize()
    {
        Application.quitting += SaveSnapshot;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        SaveSnapshot();
    }

    public void SaveSnapshot()
    {
        Debug.Log("Saving snapshot of unit positions.");
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
                        targetController.transform.position = positions[targetController.Instance.Id];
                    }
                }
            }
        }

        return positions;
    }
}