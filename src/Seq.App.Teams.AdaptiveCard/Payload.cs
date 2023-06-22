using System.Collections.Generic;

namespace Seq.App.Teams;

public sealed record Payload(
    string Id,
    string TimeStamp,
    string Level,
    string MessageTemplate,
    string Message,
    string Exception,
    IReadOnlyDictionary<string, object> Properties,
    uint EventType,
    string AppTitle,
    string InstanceName,
    string BaseUri);
