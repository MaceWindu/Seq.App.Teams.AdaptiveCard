using AdaptiveCards.Templating;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Seq.App.Teams;

public sealed partial class TeamsApp
{
    private const string WRAP_PREFIX = @"{
   ""type"":""message"",
   ""attachments"":[
      {
         ""contentType"":""application/vnd.microsoft.card.adaptive"",
         ""content"":";

    private const string WRAP_SUFFIX = @"
      }
   ]
}";

    private static readonly MediaTypeWithQualityHeaderValue _accepts = new("application/json");

    private HttpClientHandler _httpClientHandler = default!;

    private Dictionary<string, object?> BuildPayload(Event<LogEventData> evt)
    {
        var data = new Dictionary<string, object?>()
        {
            { "Id", evt.Id },
            { "TimeStamp", evt.Timestamp.ToString("O") },
            { "Level", evt.Data.Level.ToString() },
            { "MessageTemplate", evt.Data.MessageTemplate },
            { "Message", evt.Data.RenderedMessage },
            { "Exception", evt.Data.Exception },
            { "Properties", evt.Data.Properties },
            { "EventType", evt.EventType },
            { "AppTitle", App.Title },
            { "InstanceName", Host.InstanceName },
            { "BaseUri", Host.BaseUri },
        };

        foreach (var propPath in ExcludedProperties)
        {
            var abort = false;
            var currentData = data;
            for (var i = 0; i < propPath.Count && !abort; i++)
            {
                var name = propPath[i];
                if (i == propPath.Count - 1)
                {
                    _ = currentData.Remove(name);
                }
                else if (currentData.TryGetValue(name, out var value))
                {
                    if (value is Dictionary<string, object?> dict)
                    {
                        currentData = dict;
                    }
                    else if (value is IReadOnlyDictionary<string, object?> roDict)
                    {
                        currentData[name] = currentData = roDict.ToDictionary(_ => _.Key, _ => _.Value);
                    }
                    else
                    {
                        abort = true;
                    }
                }
                else
                {
                    abort = true;
                }
            }
        }

        if (TraceEnabled)
        {
            _log.Information("Template data: {json}", JsonSerializer.Serialize(data));
        }

        return data;
    }

    private async Task SendCard(string card)
    {
        using var client = new HttpClient(_httpClientHandler, disposeHandler: false)
        {
            BaseAddress = new Uri(TeamsBaseUrl)
        };

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(_accepts);

        var message = WRAP_PREFIX + card + WRAP_SUFFIX;
        using var content = new StringContent(message, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("", content).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _log.Error(
                "Could not send Teams message, server replied {StatusCode} {StatusMessage}: {Message}. Request Body: {RequestBody}",
                (int)response.StatusCode,
                response.StatusCode,
                await response.Content.ReadAsStringAsync().ConfigureAwait(false),
                message);
        }
        else
        {
            if (TraceEnabled)
            {
                var responseResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _log.Information("Server replied {StatusCode} {StatusMessage}: {Message}", response.StatusCode, (int)response.StatusCode, responseResult);
            }
        }
    }

    async Task ISubscribeToAsync<LogEventData>.OnAsync(Event<LogEventData> evt)
    {
        try
        {
            //If the event level is defined and it is not in the list do not log it
            if (LogEventLevelList.Count > 0 && !LogEventLevelList.Contains(evt.Data.Level))
            {
                return;
            }

            var template = new AdaptiveCardTemplate(string.IsNullOrWhiteSpace(CardTemplate) ? _defaultTemplate : CardTemplate);

            var payload = BuildPayload(evt);

            if (TraceEnabled)
            {
                _log.Debug("Payload: {json}", payload);
            }

            // doesn't work currently with v2
            //var bodyJson = template.Expand(payload);
            var bodyJson = template.Expand(JsonSerializer.Serialize(payload));

            if (TraceEnabled)
            {
                _log.Information("ActionCard: {json}", bodyJson);
            }

            var warnings = template.GetLastTemplateExpansionWarnings();
            foreach (var warn in warnings)
            {
                _log.Warning("AdaptiveCard template warning: {Warning}", warn);
            }

            await SendCard(bodyJson).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "An error occurred while constructing the request");
        }
    }
}
