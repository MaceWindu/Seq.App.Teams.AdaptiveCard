using System.Text.Json.Serialization;

namespace Seq.App.Teams.Models;
/// <summary>
/// O365 connector card OpenUri target
/// </summary>
internal sealed class O365ConnectorCardOpenUriTarget
{
    /// <summary>
    /// Gets or sets target operating system. Possible values include:
    /// 'default', 'iOS', 'android', 'windows'
    /// </summary>
    [JsonPropertyName("os")]
    public string? Os { get; set; }

    /// <summary>
    /// Gets or sets target url
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

}
