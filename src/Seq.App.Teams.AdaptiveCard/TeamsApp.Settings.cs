using Seq.Apps;

namespace Seq.App.Teams;

public sealed partial class TeamsApp
{
    [SeqAppSetting(
        DisplayName = "Teams WebHook URL",
        HelpText = "Used to send message to Teams. This can be retrieved by adding a Incoming Webhook connector to your Teams channel.")]
    public string? TeamsBaseUrl { get; set; }

    [SeqAppSetting(
        DisplayName = "Proxy Server",
        HelpText = "Address of proxy server for WebHook connection.",
        IsOptional = true)]
    public string? WebProxy { get; set; }

    [SeqAppSetting(
        DisplayName = "Proxy user name",
        HelpText = "Proxy server user name",
        IsOptional = true)]
    public string? WebProxyUserName { get; set; }

    [SeqAppSetting(
        DisplayName = "Proxy password",
        HelpText = "Proxy server password",
        IsOptional = true,
        InputType = SettingInputType.Password)]
    public string? WebProxyPassword { get; set; }

    [SeqAppSetting(
        DisplayName = "Enable Trace Logs",
        HelpText = "Log trace/debug messages.",
        IsOptional = true)]
    public bool TraceEnabled { get; set; }

    [SeqAppSetting(
        DisplayName = "Log only specified event levels",
        IsOptional = true,
        HelpText = "When set, only events with specified levels are sent to Teams. Valid Values: Verbose,Debug,Information,Warning,Error,Fatal (comma-separated)")]
    public string? LogEventLevels { get; set; }

    [SeqAppSetting(
        DisplayName = "AdaptiveCard template",
        HelpText = "You can use AdaptiveCard designer at https://adaptivecards.io/designer/ to design your card",
        InputType = SettingInputType.LongText,
        IsOptional = true)]
    public string? CardTemplate { get; set; }

    [SeqAppSetting(
        DisplayName = "Excluded Properties",
        HelpText = "Specify properties to exclude from template model. Each property should be specified on separate line. Format: [property-name]+. `\\`, `]`, `\\r` and `\\n` symbols in property name should be escaped with \\: `\\\\`, `\\]`, `\\r`, `\\n`",
        InputType = SettingInputType.LongText,
        IsOptional = true)]
    public string? PropertiesToExclude { get; set; }
}
