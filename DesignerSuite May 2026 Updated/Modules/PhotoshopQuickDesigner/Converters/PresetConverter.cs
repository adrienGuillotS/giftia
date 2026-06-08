using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSQuickDesigner;

public class PresetConverter : JsonConverter<Preset>
{
    public override Preset ReadJson(JsonReader reader, Type objectType, Preset existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var preset = new Preset();

        // Deserialize fields with possible type conversions
        preset.Name = jsonObject["name"]?.ToString();
        preset.Identifier = jsonObject["identifier"]?.ToString();
        preset.Group = jsonObject["group"]?.ToString();
        preset.Width = jsonObject["width"]?.ToObject<double>() ?? 0.0;
        preset.Height = jsonObject["height"]?.ToObject<double>() ?? 0.0;
        preset.Units = jsonObject["units"]?.ToString();
        preset.Profile = jsonObject["profile"]?.ToString();

        // Handle possible type conversion for resolution
        var resolutionToken = jsonObject["resolution"];
        if (resolutionToken != null && float.TryParse(resolutionToken.ToString(), out float resolution))
        {
            preset.Resolution = resolution;
        }
        else
        {
            preset.Resolution = 300f;
        }

        preset.ResolutionUnits = jsonObject["resolutionUnits"]?.ToString();
        preset.Depth = jsonObject["depth"]?.ToObject<int>() ?? 0;
        preset.Scale = jsonObject["scale"]?.ToObject<double>() ?? 0.0;
        preset.Mode = jsonObject["mode"]?.ToString();
        preset.Fill = jsonObject["fill"]?.ToString();
        preset.Guides = jsonObject["guides"]?.ToObject<List<object>>() ?? new List<object>();
        preset.Artboards = jsonObject["artboards"]?.ToObject<List<Artboard>>() ?? new List<Artboard>();
        preset.LastUsedTime = jsonObject["lastUsedTime"]?.ToObject<long>() ?? 0;

        return preset;
    }

    public override void WriteJson(JsonWriter writer, Preset value, JsonSerializer serializer)
    {
        JObject jsonObject = new JObject
    {
        { "name", value.Name ?? (JToken)JValue.CreateNull() },
        { "identifier", value.Identifier ?? (JToken)JValue.CreateNull() },
        { "group", value.Group ?? (JToken)JValue.CreateNull() },
        { "width", value.Width },
        { "height", value.Height },
        { "units", value.Units ?? (JToken)JValue.CreateNull() },
        { "profile", value.Profile ?? (JToken)JValue.CreateNull() },
        { "resolution", value.Resolution },
        { "resolutionUnits", value.ResolutionUnits ?? (JToken)JValue.CreateNull() },
        { "depth", value.Depth },
        { "scale", value.Scale },
        { "mode", value.Mode ?? (JToken)JValue.CreateNull() },
        { "fill", value.Fill?.ToString() ?? (JToken)JValue.CreateNull() },
        { "guides", value.Guides != null ? JArray.FromObject(value.Guides) : new JArray() },
        { "artboards", value.Artboards != null ? JArray.FromObject(value.Artboards) : new JArray() },
        { "lastUsedTime", value.LastUsedTime }
    };

        jsonObject.WriteTo(writer);
    }


}