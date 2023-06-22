using Seq.Apps;

namespace Seq.App.Teams;

public sealed partial class TeamsApp
{
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
        DisplayName = "Color",
        HelpText = "Hex theme color for messages (ex. ff0000). (default: auto based on message level)",
        IsOptional = true)]
    public string? Color { get; set; }

    [SeqAppSetting(
        DisplayName = "Comma seperated list of event levels",
        IsOptional = true,
        HelpText = "If specified Teams card will be created only for the specified event levels, other levels will be discarded (useful for streaming events). Valid Values: Verbose,Debug,Information,Warning,Error,Fatal")]
    public string? LogEventLevels { get; set; }


    [SeqAppSetting(
    DisplayName = "AdaptiveCard template",
    HelpText = "You can use AdaptiveCard designer at https://adaptivecards.io/designer/ to design your card",
    InputType = SettingInputType.LongText,
    IsOptional = true)]
    public string? CardTemplate { get; set; }
}
