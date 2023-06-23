using AdaptiveCards.Templating;
using Newtonsoft.Json;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

    private Payload BuildPayload(Event<LogEventData> evt)
    {
        var data = new Payload(
            evt.Id,
            evt.TimestampUtc.ToString("O"),
            evt.Data.Level.ToString(),
            evt.Data.MessageTemplate,
            evt.Data.RenderedMessage,
            evt.Data.Exception,
            evt.Data.Properties,
            evt.EventType,
            App.Title,
            Host.InstanceName,
            Host.BaseUri);

        _log.Information("Payload {json}", JsonConvert.SerializeObject(data));

        return data;
    }

    private async Task SendCart(string card)
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
            if (TraceMessage)
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
            if (LogEventLevelList?.Count > 0 && !LogEventLevelList.Contains(evt.Data.Level))
            {
                return;
            }

            if (TraceMessage)
            {
                _log.Information("Start Processing {Message}", evt.Data.RenderedMessage);
            }

            var template = new AdaptiveCardTemplate(string.IsNullOrWhiteSpace(CardTemplate) ? _defaultTemplate : CardTemplate);
            var bodyJson = template.Expand(BuildPayload(evt));

            _log.Information("ActionCard {json}", bodyJson);

            var warnings = template.GetLastTemplateExpansionWarnings();
            foreach (var warn in warnings)
            {
                _log.Warning("Teams template {Warning}", warn);
            }

            await SendCart(bodyJson).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "An error occured while constructing the request");
        }
    }
}
