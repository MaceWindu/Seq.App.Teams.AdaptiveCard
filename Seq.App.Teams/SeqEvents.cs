using Seq.Apps;
using Seq.Apps.LogEvents;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Seq.App.Teams;

public static class SeqEvents
{
    private const uint AlertV1EventType = 0xA1E77000, AlertV2EventType = 0xA1E77001;

    private static readonly JsonSerializerOptions _serializationOptionsCompact = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions _serializationOptionsIndented = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public static (string description, string href) GetOpenLink(string seqBaseUrl, Event<LogEventData> evt)
    {
        var config = new PropertyConfig { SkipEscapeMarkDown = true };

        return evt.EventType == AlertV1EventType
            ? ((string description, string href))("Open Seq Alert", GetProperty(evt, "ResultsUrl", config))
            : evt.EventType == AlertV2EventType
                ? ((string description, string href))("Open Seq Alert", GetProperty(evt, "Source.ResultsUrl", config))
                : ((string description, string href))("Open Seq Event", UILinkTo(seqBaseUrl, evt));
    }

    public static string GetProperty(Event<LogEventData> evt, string propertyPath, PropertyConfig? config = null)
    {
        var properties = GetProperties(evt, config ?? new PropertyConfig());

        return properties.TryGetValue(propertyPath, out var value) ? value : "";
    }

    internal static Dictionary<string, string> GetProperties(Event<LogEventData> evt, PropertyConfig config)
    {
        return GetProperties(evt.Data.Properties, config, "").ToDictionary(x => x.Key, x => x.Value);
    }

    public static string UILinkTo(string seqBaseUrl, Event<LogEventData> evt)
    {
        return $"{seqBaseUrl.TrimEnd('/')}/#/events?filter=@Id%20%3D%3D%20%22{evt.Id}%22&show=expanded";
    }

    private static IEnumerable<KeyValuePair<string, string>> GetProperties(IReadOnlyDictionary<string, object> properties, PropertyConfig config, string parentPropertyPath)
    {
        if (properties == null)
        {
            yield break;
        }

        foreach (var property in properties)
        {
            var propertyPath = $"{parentPropertyPath}.{property.Key}".TrimStart(new[] { '.' });

            if (config.ExcludedProperties.Contains(propertyPath))
            {
                continue;
            }

            if (property.Value is IReadOnlyDictionary<string, object> childProperties && !config.JsonSerializedProperties.Contains(propertyPath))
            {
                foreach (var nestedProperty in GetProperties(childProperties, config, propertyPath))
                {
                    yield return nestedProperty;
                }
            }
            else
            {
                yield return new KeyValuePair<string, string>(propertyPath, GetPropertyValue(propertyPath, property, config));
            }
        }
    }

    private static string GetPropertyValue(string propertyPath, KeyValuePair<string, object> property, PropertyConfig config)
    {
        string value;

        if (property.Value is null)
        {
            return "`null`";
        }
        else if (config.JsonSerializedProperties.Contains(propertyPath))
        {
            var settings = config.JsonSerializedPropertiesAsIndented ? _serializationOptionsIndented : _serializationOptionsCompact;

            value = JsonSerializer.Serialize(property.Value, options: settings);
        }
        else
        {
            value = property.Value.ToString();
        }

        return config.SkipEscapeMarkDown ? value : value.EscapeMarkdown();
    }
}
