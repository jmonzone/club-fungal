using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class LocalData : ScriptableObject
{
    [SerializeField] private bool resetDataOnAwake;
    [SerializeField] private JObject json;
    public JObject JsonFile => json;

    public event UnityAction OnReset;
    public static string GetSaveDataPath()
    {
        string path = $"{Application.persistentDataPath}/data.json";
        return path;
    }

    public void Initialize()
    {
        var saveDataPath = GetSaveDataPath();

        if (!File.Exists(saveDataPath)) json = new JObject();
        else
        {
            try
            {
                var configFile = File.ReadAllText(saveDataPath);
                json = JObject.Parse(configFile);
            }
            catch
            {
                json = new JObject();
            }
        }
    }

    public void SaveData(string key, JToken value)
    {
        Initialize();
        var saveDataPath = GetSaveDataPath();
        json[key] = value;
        Directory.CreateDirectory(Path.GetDirectoryName(saveDataPath));
        File.WriteAllText(saveDataPath, json.ToString());
    }

    public void ResetData()
    {
        var saveDataPath = GetSaveDataPath();
        json = new JObject();
        Directory.CreateDirectory(Path.GetDirectoryName(saveDataPath));
        File.WriteAllText(saveDataPath, JsonFile.ToString());
        OnReset?.Invoke();
    }
}