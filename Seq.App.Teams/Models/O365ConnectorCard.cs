using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Seq.App.Teams.Models;
/// <summary>
/// O365 connector card
/// </summary>
internal class O365ConnectorCard
{
    /// <summary>
    /// Gets or sets title of the item
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets text for the card
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets summary for the card
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets theme color for the card
    /// </summary>
    [JsonPropertyName("themeColor")]
    public string? ThemeColor { get; set; }

    /// <summary>
    /// Gets or sets set of sections for the current card
    /// </summary>
    [JsonPropertyName("sections")]
    public IList<O365ConnectorCardSection>? Sections { get; set; }

    /// <summary>
    /// Gets or sets set of actions for the current card
    /// </summary>
    [JsonPropertyName("potentialAction")]
    public IList<O365ConnectorCardActionBase>? PotentialAction { get; set; }

}
