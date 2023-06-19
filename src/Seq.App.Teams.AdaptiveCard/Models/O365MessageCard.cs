using System.Text.Json.Serialization;

namespace Seq.App.Teams.Models;

internal sealed class O365MessageCard : O365ConnectorCard
{
    [JsonPropertyName("@context")]
#pragma warning disable CA1822 // Mark members as static
    public string Context => "http://schema.org/extensions";
#pragma warning restore CA1822 // Mark members as static
    [JsonPropertyName("@type")]
#pragma warning disable CA1822 // Mark members as static
    public string Type => "MessageCard";
#pragma warning restore CA1822 // Mark members as static
}
