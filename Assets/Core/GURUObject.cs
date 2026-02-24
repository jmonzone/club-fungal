using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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

/// <summary>
/// Generic converter for List<T> where T : GURUObject.
/// Serializes to list of IDs, deserializes by looking up in provided collection.
/// </summary>
public class GURUListConverter<T> : JsonConverter<List<T>> where T : GURUObject
{
    private readonly List<T> _collection;

    public GURUListConverter(List<T> collection)
    {
        _collection = collection;
    }

    public override void WriteJson(JsonWriter writer, List<T> value, JsonSerializer serializer)
    {
        if (value == null || value.Count == 0)
        {
            writer.WriteNull();
            return;
        }

        var ids = value.Select(item => item?.Id).Where(id => !string.IsNullOrEmpty(id)).ToList();
        serializer.Serialize(writer, ids);
    }

    public override List<T> ReadJson(JsonReader reader, System.Type objectType, List<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return new List<T>();
        }

        var ids = serializer.Deserialize<List<string>>(reader);
        if (ids == null || ids.Count == 0 || _collection == null)
        {
            return new List<T>();
        }

        var items = new List<T>();
        foreach (var id in ids)
        {
            var item = _collection.FirstOrDefault(obj => obj.Id == id);
            if (item != null)
            {
                items.Add(item);
            }
        }

        return items;
    }
}
