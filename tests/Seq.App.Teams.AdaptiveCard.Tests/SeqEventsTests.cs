using NUnit.Framework;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;

namespace Seq.App.Teams.Tests;

public sealed class SeqEventsTests
{
    [TestCase("Zero", "")]
    [TestCase("First", "`null`")]
    [TestCase("Second", "20")]
    [TestCase("Third", "")]
    [TestCase("Third.Fourth", "test 1")]
    [TestCase("Third.Fifth", "\\{\"Sixth\":\"test 2\",\"Seventh\":123\\}")]
    [TestCase("Eighth", "")]
    [TestCase("Eighth.Excluded", "")]
    public void PropertiesAreRetrievedFromTheEvent(string propertyPath, string expectedMarkdown)
    {
        var data = new LogEventData
        {
            Properties = new Dictionary<string, object?>
            {
                ["First"] = null,
                ["Second"] = 20,
                ["Third"] = new Dictionary<string, object>
                {
                    ["Fourth"] = "test 1",
                    ["Fifth"] = new Dictionary<string, object>
                    {
                        ["Sixth"] = "test 2",
                        ["Seventh"] = 123
                    }
                },
                ["Eighth"] = new Dictionary<string, object>
                {
                    ["Excluded"] = true
                }
            }
        };

        var evt = new Event<LogEventData>("event-123", 4, DateTime.UtcNow, data);
        var config = new PropertyConfig
        {
            ExcludedProperties = new List<string>
            {
                "Eighth"
            },
            JsonSerializedProperties = new List<string>
            {
                "Third.Fifth"
            }
        };

        var actual = SeqEvents.GetProperty(evt, propertyPath, config);
        Assert.That(actual, Is.EqualTo(expectedMarkdown));
    }

    [TestCase("https://example.com", "event-123", "https://example.com/#/events?filter=@Id%20%3D%3D%20%22event-123%22&show=expanded")]
    [TestCase("https://example.com/", "event-123", "https://example.com/#/events?filter=@Id%20%3D%3D%20%22event-123%22&show=expanded")]
    [TestCase("https://example.com/test", "event-123", "https://example.com/test/#/events?filter=@Id%20%3D%3D%20%22event-123%22&show=expanded")]
    [TestCase("https://example.com/test/", "event-123", "https://example.com/test/#/events?filter=@Id%20%3D%3D%20%22event-123%22&show=expanded")]
    public void EventLinksAreGenerated(string seqBaseUrl, string eventId, string expectedLink)
    {
        var actual = SeqEvents.UILinkTo(
            seqBaseUrl,
            new Event<LogEventData>(eventId, 0, DateTime.UtcNow, new LogEventData()));

        Assert.That(actual, Is.EqualTo(expectedLink));
    }

    [TestCase((uint)1, "Open Seq Event")]
    [TestCase(0xA1E77000, "Open Seq Alert")]
    [TestCase(0xA1E77001, "Open Seq Alert")]
    public void CorrectOpenLinkTitleIsIdentified(uint eventType, string expectedLinkTitle)
    {
        var (title, _) = SeqEvents.GetOpenLink(
            "https://example.com",
            new Event<LogEventData>("event-1", eventType, DateTime.UtcNow, new LogEventData()));

        Assert.That(title, Is.EqualTo(expectedLinkTitle));
    }
}
