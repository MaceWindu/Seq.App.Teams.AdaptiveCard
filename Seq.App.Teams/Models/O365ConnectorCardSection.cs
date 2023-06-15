using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Seq.App.Teams.Models;
/// <summary>
/// O365 connector card section
/// </summary>
internal sealed class O365ConnectorCardSection
{

    /// <summary>
    /// Gets or sets title of the section
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets text for the section
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets activity title
    /// </summary>
    [JsonPropertyName("activityTitle")]
    public string? ActivityTitle { get; set; }

    /// <summary>
    /// Gets or sets activity subtitle
    /// </summary>
    [JsonPropertyName("activitySubtitle")]
    public string? ActivitySubtitle { get; set; }

    /// <summary>
    /// Gets or sets activity text
    /// </summary>
    [JsonPropertyName("activityText")]
    public string? ActivityText { get; set; }

    /// <summary>
    /// Gets or sets activity image
    /// </summary>
    [JsonPropertyName("activityImage")]
    public string? ActivityImage { get; set; }

    /// <summary>
    /// Gets or sets describes how Activity image is rendered. Possible
    /// values include: 'avatar', 'article'
    /// </summary>
    [JsonPropertyName("activityImageType")]
    public string? ActivityImageType { get; set; }

    /// <summary>
    /// Gets or sets use markdown for all text contents. Default vaule is
    /// true.
    /// </summary>
    [JsonPropertyName("markdown")]
    public bool? Markdown { get; set; }

    /// <summary>
    /// Gets or sets set of facts for the current section
    /// </summary>
    [JsonPropertyName("facts")]
    public IList<O365ConnectorCardFact>? Facts { get; set; }

    [JsonPropertyName("startGroup")]
    public bool StartGroup { get; set; }

}
