using Newtonsoft.Json;
using UnityEngine;

public abstract class GURUObject : ScriptableObject
{
    [SerializeField] private string id;
    public string Id => id;

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = name.ToLower();
        }
    }
}

public class GURUConverter : JsonConverter<GURUObject>
{
    public override void WriteJson(JsonWriter writer, GURUObject value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(value.Id);
        }
    }

    public override GURUObject ReadJson(JsonReader reader, System.Type objectType, GURUObject existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // During deserialization, return null - the service will resolve it from the ID
        return null;
    }
}
