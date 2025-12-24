using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class UnitSpeciesConverter : JsonConverter<UnitSpecies>
{
    private readonly List<UnitSpecies> _speciesCollection;

    public UnitSpeciesConverter(List<UnitSpecies> speciesCollection)
    {
        _speciesCollection = speciesCollection;
    }

    public override void WriteJson(JsonWriter writer, UnitSpecies value, JsonSerializer serializer)
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

    public override UnitSpecies ReadJson(JsonReader reader, System.Type objectType, UnitSpecies existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var speciesId = reader.Value?.ToString();
        if (string.IsNullOrEmpty(speciesId) || _speciesCollection == null)
        {
            return null;
        }

        return _speciesCollection.FirstOrDefault(s => s.Id == speciesId);
    }
}
