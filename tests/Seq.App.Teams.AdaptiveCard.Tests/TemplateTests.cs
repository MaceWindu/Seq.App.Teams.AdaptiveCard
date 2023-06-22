using AdaptiveCards.Templating;
using NUnit.Framework;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;
using System.IO;

namespace Seq.App.Teams.Tests;

public sealed class TemplateTests
{
    private static readonly Event<LogEventData> _event = new(
        id: "event-id",
        eventType: uint.MaxValue,
        timestampUtc: new DateTime(2020, 1, 22, 18, 19, 59, 123, DateTimeKind.Utc).AddTicks(4567),
        new LogEventData()
        {
            Id = "some-other-id",
            LocalTimestamp = new DateTimeOffset(2020, 1, 22, 18, 19, 59, 123, 456, TimeSpan.FromMinutes(45)).AddTicks(7),
            Level = LogEventLevel.Fatal,
            MessageTemplate = "template {message}",
            RenderedMessage = "rendered message",
            Exception = @"exception
data",
            Properties = new Dictionary<string, object?>()
            {
                { "#FieldString", "my string" },
                { "#FieldNumber", -123 },
                { "#FieldBoolean", true },
                { "#FieldNull", null },
                { "#FieldSimpleStringArray", new string[] { "1", "2", "3" } },
                { "#FieldSimpleIntArray", new int[] { 1, 2, 3 } },
                { "#FieldObjectArray", new object[] { new { one = 1, two = 2 }, new { one = -1, two = -2 }}},
            }
        });


    [Test]
    public void TestDefaultTemplate()
    {
        var data = new Payload(
            _event.Id,
            _event.TimestampUtc.ToString("O"),
            _event.Data.Level.ToString(),
            _event.Data.MessageTemplate,
            _event.Data.RenderedMessage,
            _event.Data.Exception,
            _event.Data.Properties,
            _event.EventType,
            "app title",
            "instance name",
            "http://localhost");

        using var stream = typeof(TeamsApp).Assembly.GetManifestResourceStream("Seq.App.Teams.AdaptiveCard.Resources.default-template.json")!;
        using var reader = new StreamReader(stream);
        var template = reader.ReadToEnd();

        var tmpl = new AdaptiveCardTemplate(template);
        var result = tmpl.Expand(data);
        var errors = tmpl.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result, Is.Not.Null.Or.Empty);
    }
}
