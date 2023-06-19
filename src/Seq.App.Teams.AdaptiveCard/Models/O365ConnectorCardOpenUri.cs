using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Seq.App.Teams.Models;
/// <summary>
/// O365 connector card OpenUri action
/// </summary>
internal sealed class O365ConnectorCardOpenUri : O365ConnectorCardActionBase
{
    /// <summary>
    /// Gets or sets target os / urls
    /// </summary>
    [JsonPropertyName("targets")]
    public IList<O365ConnectorCardOpenUriTarget>? Targets { get; set; }

}
