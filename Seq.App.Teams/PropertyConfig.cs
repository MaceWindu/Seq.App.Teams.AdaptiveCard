using System.Collections.Generic;

namespace Seq.App.Teams;

public sealed class PropertyConfig
{
    public List<string> JsonSerializedProperties { get; set; } = new List<string>();
    public List<string> ExcludedProperties { get; set; } = new List<string>();
    internal bool JsonSerializedPropertiesAsIndented { get; set; }
    internal bool SkipEscapeMarkDown { get; set; }
}
