using Seq.Apps.LogEvents;
using System.Collections.Generic;

namespace Seq.App.Teams;

internal sealed record Payload(
    string Id,
    string TimeStamp,
    LogEventLevel Level,
    string MessageTemplate,
    string RenderedMessage,
    string Exception,
    IReadOnlyDictionary<string, object> Properties,
    uint EventType,
    string AppTitle,
    string InstanceName,
    string BaseUri);
