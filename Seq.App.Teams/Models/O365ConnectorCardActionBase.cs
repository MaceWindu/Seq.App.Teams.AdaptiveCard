using System.Text.Json.Serialization;

namespace Seq.App.Teams.Models;
/// <summary>
/// O365 connector card action base
/// </summary>
internal class O365ConnectorCardActionBase
{
    /// <summary>
    /// Gets or sets type of the action (ActionCard, HttpPOST, or OpenUri)
    /// https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/connectors/connectors-using
    /// </summary>
    [JsonPropertyName("@type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets name of the action that will be used as button title
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets action Id
    /// </summary>
    [JsonPropertyName("@id")]
    public string? Id { get; set; }

}
