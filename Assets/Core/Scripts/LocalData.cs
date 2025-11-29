using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class LocalData : ScriptableObject
{
    [SerializeField] private bool resetDataOnAwake;
    public JObject JsonFile { get; private set; }

    public event UnityAction OnReset;
    public static string GetSaveDataPath()
    {
        string path = $"{Application.persistentDataPath}/data.json";
        return path;
    }

    public void Initialize()
    {
        var saveDataPath = GetSaveDataPath();

        Directory.CreateDirectory(Path.GetDirectoryName(saveDataPath));

        if (Application.isEditor && resetDataOnAwake) ResetData();

        if (!File.Exists(saveDataPath)) JsonFile = new JObject();
        else
        {
            try
            {
                var configFile = File.ReadAllText(saveDataPath);
                JsonFile = JObject.Parse(configFile);
            }
            catch
            {
                JsonFile = new JObject();
            }
        }
    }

    public void SaveData(string key, JToken value)
    {
        var saveDataPath = GetSaveDataPath();
        JsonFile[key] = value;
        Directory.CreateDirectory(Path.GetDirectoryName(saveDataPath));
        File.WriteAllText(saveDataPath, JsonFile.ToString());
    }

    public void ResetData()
    {
        var saveDataPath = GetSaveDataPath();
        JsonFile = new JObject();
        Directory.CreateDirectory(Path.GetDirectoryName(saveDataPath));
        File.WriteAllText(saveDataPath, JsonFile.ToString());
        OnReset?.Invoke();
    }
}
