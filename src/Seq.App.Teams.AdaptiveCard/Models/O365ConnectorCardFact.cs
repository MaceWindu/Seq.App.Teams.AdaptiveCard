using System.Text.Json.Serialization;

namespace Seq.App.Teams.Models;
/// <summary>
/// O365 connector card fact
/// </summary>
internal sealed class O365ConnectorCardFact
{
    /// <summary>
    /// Gets or sets display name of the fact
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets display value for the fact
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

}
