using Seq.App.Teams.Models;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Seq.App.Teams;

[SeqApp("Teams",
Description = "Sends events and notifications to Microsoft Teams.")]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public sealed class TeamsApp : SeqApp, ISubscribeToAsync<LogEventData>
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private const string ExcludeAllPropertyName = "All";

    private static readonly JsonSerializerOptions _serializationOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly IDictionary<LogEventLevel, string> LevelColorMap = new Dictionary<LogEventLevel, string>
    {
        {LogEventLevel.Verbose, "808080"},
        {LogEventLevel.Debug, "808080"},
        {LogEventLevel.Information, "008000"},
        {LogEventLevel.Warning, "ffff00"},
        {LogEventLevel.Error, "ff0000"},
        {LogEventLevel.Fatal, "ff0000"}
    };

    private HttpClientHandler _httpClientHandler = default!;
    private ILogger _log = default!;

    #region "Settings"

    [SeqAppSetting(
        DisplayName = "Seq Base URL",
        HelpText = "Used for generating links to events in Teams messages; if not specified, Seq's configured base URL will be used.",
        IsOptional = true)]
    public string? BaseUrl { get; set; }

    [SeqAppSetting(
        DisplayName = "Web proxy",
        HelpText = "When a web proxy is present in the network for connecting to outside URLs.",
        IsOptional = true)]
    public string? WebProxy { get; set; }

    [SeqAppSetting(
        DisplayName = "Web proxy user name",
        HelpText = "Proxy user name, if authorization required",
        IsOptional = true)]
    public string? WebProxyUserName { get; set; }

    [SeqAppSetting(
        DisplayName = "Web proxy password",
        HelpText = "Proxy password, if authorization required",
        IsOptional = true,
        InputType = SettingInputType.Password)]
    public string? WebProxyPassword { get; set; }

    [SeqAppSetting(
        DisplayName = "Teams WebHook URL",
        HelpText = "Used to send message to Teams. This can be retrieved by adding a Incoming Webhook connector to your Teams channel.")]
    public string? TeamsBaseUrl { get; set; }

    [SeqAppSetting(
        DisplayName = "Trace All Messages",
        HelpText = "Used to show all messages to trace; note that this will cause the Teams Webhook URL to appear in diagnostic messages.",
        IsOptional = true)]
    public bool TraceMessage { get; set; }

    [SeqAppSetting(
        DisplayName = "Properties to exclude",
        HelpText = "The properties that will be excluded from the messages. Use '" + ExcludeAllPropertyName + "' to exclude all Seq properties. Multiple properties can be specified; enter one per line.",
        IsOptional = true)]
    public string? ExcludedProperties { get; set; }

    [SeqAppSetting(
        DisplayName = "Properties to serialize as JSON",
        HelpText = "The properties that should be serialized as JSON instead of the native ToString() on the value. Multiple properties can be specified; enter one per line.",
        InputType = SettingInputType.LongText,
        IsOptional = true)]
    public string? JsonSerializedProperties { get; set; }

    [SeqAppSetting(
        DisplayName = "Properties to serialize as JSON - Use Indented JSON?",
        HelpText = "For properties that are serialized as JSON, should they be indented?",
        InputType = SettingInputType.Checkbox,
        IsOptional = true)]
    public bool JsonSerializedPropertiesAsIndented { get; set; }

    [SeqAppSetting(
        DisplayName = "Color",
        HelpText = "Hex theme color for messages (ex. ff0000). (default: auto based on message level)",
        IsOptional = true)]
    public string? Color { get; set; }

    [SeqAppSetting(
        DisplayName = "Comma seperated list of event levels",
        IsOptional = true,
        HelpText = "If specified Teams card will be created only for the specified event levels, other levels will be discarded (useful for streaming events). Valid Values: Verbose,Debug,Information,Warning,Error,Fatal")]
    public string? LogEventLevels { get; set; }

    #endregion

    /// <inheritdoc />
    protected override void OnAttached()
    {
        _log = TraceMessage ? Log.ForContext("Uri", TeamsBaseUrl) : Log;

        _httpClientHandler = new HttpClientHandler();
        if (!string.IsNullOrEmpty(WebProxy))
        {
            ICredentials? credentials = null;
            if (!string.IsNullOrEmpty(WebProxyUserName))
            {
                credentials = new NetworkCredential(WebProxyUserName, WebProxyPassword);
            }
            _httpClientHandler.Proxy = new WebProxy(WebProxy, false, null, credentials);
            _httpClientHandler.UseProxy = true;
        }
        else
        {
            _httpClientHandler.UseProxy = false;
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

            using var client = new HttpClient(_httpClientHandler, disposeHandler: false);
            client.BaseAddress = new Uri(TeamsBaseUrl);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            O365ConnectorCard body = BuildBody(evt);
            var bodyJson = JsonSerializer.Serialize(body, options: _serializationOptions);
            using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("", content)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _log.Error("Could not send Teams message, server replied {StatusCode} {StatusMessage}: {Message}. Request Body: {RequestBody}", (int)response.StatusCode, response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false), bodyJson);
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
        catch (Exception ex)
        {
            _log.Error(ex, "An error occured while constructing the request");
        }
    }

    private HashSet<LogEventLevel> LogEventLevelList
    {
        get
        {
            var result = new HashSet<LogEventLevel>();
            if (string.IsNullOrEmpty(LogEventLevels))
            {
                return result;
            }

            var strValues = LogEventLevels!.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (!(strValues?.Length > 0))
            {
                return result;
            }

            foreach (var strValue in strValues)
            {
                if (Enum.TryParse(strValue, out LogEventLevel enumValue))
                {
                    _ = result.Add(enumValue);
                }
            }

            return result;
        }
    }

    private O365MessageCard BuildBody(Event<LogEventData> evt)
    {
        // Build action
        var url = BaseUrl;
        if (string.IsNullOrWhiteSpace(url))
        {
            url = Host.BaseUri;
        }

        var (openTitle, openUrl) = SeqEvents.GetOpenLink(url!, evt);
        var action = new O365ConnectorCardOpenUri
        {
            Name = openTitle,
            Type = "OpenUri", //Failure to provide this will cause a 400 badrequest
            Targets = new[]
            {
                new O365ConnectorCardOpenUriTarget()
                {
                    Uri = openUrl,
                    Os = "default" //Failure to provide this will cause a 400 badrequest
                }
            }
        };

        // Build message
        var msg = evt.Data.RenderedMessage;
        if (msg.Length > 1000)
        {
            msg = msg.EscapeMarkdown()[..1000];
        }

        var color = Color;
        if (string.IsNullOrWhiteSpace(color))
        {
            color = LevelColorMap[evt.Data.Level];
        }

        var body = new O365MessageCard()
        {
            Title = evt.Data.Level.ToString().EscapeMarkdown(),
            ThemeColor = color!,
            Text = msg,
            PotentialAction = new O365ConnectorCardActionBase[]
            {
                action
            }
        };

        // Build sections
        var sections = new List<O365ConnectorCardSection>();
        var config = new PropertyConfig
        {
            ExcludedProperties = ExcludedProperties?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
            JsonSerializedProperties = JsonSerializedProperties?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
            JsonSerializedPropertiesAsIndented = JsonSerializedPropertiesAsIndented
        };

        if (!config.ExcludedProperties.Contains(ExcludeAllPropertyName))
        {
            var properties = SeqEvents.GetProperties(evt, config);

            var facts = properties.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => new O365ConnectorCardFact
            {
                Name = x.Key,
                Value = x.Value
            }).ToArray();

            if (facts.Length != 0)
            {
                sections.Add(new O365ConnectorCardSection { Facts = facts });
            }
        }

        if (!string.IsNullOrWhiteSpace(evt.Data.Exception))
        {
            sections.Add(new O365ConnectorCardSection { Title = "Exception", Text = evt.Data.Exception.EscapeMarkdown() });
        }

        body.Sections = sections.ToArray();

        return body;
    }
}
