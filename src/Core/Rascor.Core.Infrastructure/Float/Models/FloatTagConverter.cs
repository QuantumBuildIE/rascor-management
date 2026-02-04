using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rascor.Core.Infrastructure.Float.Models;

/// <summary>
/// Custom JSON converter for FloatTag that handles both string and object formats.
/// Float API returns tags as simple strings in some endpoints (e.g., people, projects)
/// and as full objects in others (e.g., dedicated tags endpoint).
/// </summary>
public class FloatTagConverter : JsonConverter<FloatTag>
{
    public override FloatTag? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                // Tag is a simple string - create FloatTag with just the name
                var tagName = reader.GetString();
                return new FloatTag { Name = tagName ?? string.Empty };

            case JsonTokenType.StartObject:
                // Tag is a full object - deserialize normally
                var tag = new FloatTag();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName?.ToLowerInvariant())
                        {
                            case "id":
                                if (reader.TokenType == JsonTokenType.Number)
                                    tag.Id = reader.GetInt32();
                                else if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out var id))
                                    tag.Id = id;
                                break;
                            case "name":
                                tag.Name = reader.GetString() ?? string.Empty;
                                break;
                            default:
                                // Store unknown properties in ExtensionData
                                tag.ExtensionData ??= new Dictionary<string, JsonElement>();
                                tag.ExtensionData[propertyName ?? "unknown"] = JsonElement.ParseValue(ref reader);
                                break;
                        }
                    }
                }
                return tag;

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} when parsing FloatTag");
        }
    }

    public override void Write(Utf8JsonWriter writer, FloatTag value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Id.HasValue)
        {
            writer.WriteNumber("id", value.Id.Value);
        }

        writer.WriteString("name", value.Name);

        // Write any extension data
        if (value.ExtensionData != null)
        {
            foreach (var kvp in value.ExtensionData)
            {
                writer.WritePropertyName(kvp.Key);
                kvp.Value.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Converter for List of FloatTag that handles arrays containing mixed string/object formats.
/// </summary>
public class FloatTagListConverter : JsonConverter<List<FloatTag>>
{
    private readonly FloatTagConverter _tagConverter = new();

    public override List<FloatTag>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return new List<FloatTag>();

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Expected array for tags, got {reader.TokenType}");

        var tags = new List<FloatTag>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            var tag = _tagConverter.Read(ref reader, typeof(FloatTag), options);
            if (tag != null)
                tags.Add(tag);
        }

        return tags;
    }

    public override void Write(Utf8JsonWriter writer, List<FloatTag> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var tag in value)
        {
            _tagConverter.Write(writer, tag, options);
        }
        writer.WriteEndArray();
    }
}
