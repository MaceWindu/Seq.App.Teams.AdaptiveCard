using AdaptiveCards.Templating;
using NUnit.Framework;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Seq.App.Teams.Tests;

internal sealed class TemplateTests
{
    private static readonly Event<LogEventData> _event = new(
        id: "event-id",
        eventType: uint.MaxValue,
        timestamp: new DateTime(2020, 1, 22, 18, 19, 59, 123, DateTimeKind.Utc).AddTicks(4567),
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
        var data = new Dictionary<string, object?>()
        {
            { "Id", _event.Id },
            { "TimeStamp", _event.Timestamp.ToString("O") },
            { "Level", _event.Data.Level.ToString() },
            { "MessageTemplate", _event.Data.MessageTemplate },
            { "Message", _event.Data.RenderedMessage },
            { "Exception", _event.Data.Exception },
            { "Properties", _event.Data.Properties },
            { "EventType", _event.EventType },
            { "AppTitle", "app title" },
            { "InstanceName", "instance name" },
            { "BaseUri", "http://localhost" }
        };

        using var stream = typeof(TeamsApp).Assembly.GetManifestResourceStream("Seq.App.Teams.AdaptiveCard.Resources.default-template.json")!;
        using var reader = new StreamReader(stream);
        var template = reader.ReadToEnd();

        var cardTemplate = new AdaptiveCardTemplate(template);
        var result = cardTemplate.Expand(data);
        var errors = cardTemplate.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result.Count(c => c == '$'), Is.EqualTo(1));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [Test]
    public void TestDefaultTemplateNoOptionsData()
    {
        var data = new Dictionary<string, object?>()
        {
            { "Id", _event.Id },
            { "TimeStamp", _event.Timestamp.ToString("O") },
            { "Level", _event.Data.Level.ToString() },
            { "MessageTemplate", _event.Data.MessageTemplate },
            { "Message", _event.Data.RenderedMessage },
            { "Exception", _event.Data.Exception },
            { "Properties", _event.Data.Properties },
            { "EventType", _event.EventType },
            { "AppTitle", "app title" },
            { "InstanceName", "instance name" },
            { "BaseUri", "http://localhost" }
        };

        using var stream = typeof(TeamsApp).Assembly.GetManifestResourceStream("Seq.App.Teams.AdaptiveCard.Resources.default-template.json")!;
        using var reader = new StreamReader(stream);
        var template = reader.ReadToEnd();

        var cardTemplate = new AdaptiveCardTemplate(template);
        var result = cardTemplate.Expand(data);
        var errors = cardTemplate.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result.Count(c => c == '$'), Is.EqualTo(1));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [Test]
    public void TestAlertTemplate()
    {
        var props = new Dictionary<string, object?>()
        {
            { "NamespacedAlertTitle", "Alert/Name" },
            { "Alert", new Dictionary<string, object?>()
            {
                { "Id", "alert-463"},
                {"Title", "New Alert" },
                { "Url", "http://localhost:80/#/alerts/alert-463"},
                { "OwnerUsername", "admin"},
                { "SignalExpressionDescription", null},
                {"Query", "select count(*) as count from stream group by time(1m) having count > 0 limit 100 for background" },
                { "HavingClause","count > 0"},
                {"TimeGrouping", "1 minute"}
            }},
            {"Source", new Dictionary<string, object?>()
            {
                { "RangeStart", "2023-06-22T13:05:43.3585376Z"},
                { "RangeEnd", "2023-06-22T13:06:43.3585376Z"},
                {"ResultsUrl", "http://localhost:80/#/events?q=select%20count%28%2A%29%20as%20count%20from%20stream%20group%20by%20time%281m%29%20having%20count%20%3E%200%20limit%20100%20for%20background&from=2023-06-22T13:05:43.3585376Z&to=2023-06-22T13:06:43.3585376Z" },
                { "Results", new object[]
                {
                    new object[]{ "time", "count" },
                    new object[]{ "2023-06-22T13:05:43.3585376Z", 19 },
                    new object[]{ "2023-06-22T13:05:44.3585376Z", 5 }
                } },
                { "ContributingEvents", new object[]{
                    new object[]{ "id", "timestamp", "message" },
                    new object[]{ "event-6d291458732108db9333010000000000", "2023-06-22T13:06:00.5580888Z", "event message 1" },
                    new object[]{ "event-6d291458732108db9433010000000000", "2023-06-22T13:06:00.5580888Z", "event message 2" }
                }
                }
            }},
            { "SuppressedUntil", "2023-06-22T13:08:13.3585376Z"},
            { "Failures", null }
        };

        var data = new Dictionary<string, object?>()
        {
            { "Id", _event.Id },
            { "TimeStamp", _event.Timestamp.ToString("O") },
            { "Level", _event.Data.Level.ToString() },
            { "MessageTemplate", _event.Data.MessageTemplate },
            { "Message", _event.Data.RenderedMessage },
            { "Exception", _event.Data.Exception },
            { "Properties", props },
            { "EventType", 2716299265 },
            { "AppTitle", "app title" },
            { "InstanceName", "instance name" },
            { "BaseUri", "http://localhost" }
        };

        using var stream = typeof(TeamsApp).Assembly.GetManifestResourceStream("Seq.App.Teams.AdaptiveCard.Resources.default-template.json")!;
        using var reader = new StreamReader(stream);
        var template = reader.ReadToEnd();

        var cardTemplate = new AdaptiveCardTemplate(template);
        var result = cardTemplate.Expand(data);
        var errors = cardTemplate.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result.Count(c => c == '$'), Is.EqualTo(1));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [Test]
    public void TestAlertTemplateNoOptionalData()
    {
        var props = new Dictionary<string, object?>()
        {
            { "NamespacedAlertTitle", "Alert/Name" },
            { "Alert", new Dictionary<string, object?>()
            {
                { "Id", "alert-463"},
                {"Title", "New Alert" },
                { "Url", "http://localhost:80/#/alerts/alert-463"},
                { "OwnerUsername", "admin"},
                { "SignalExpressionDescription", null},
                {"Query", "select count(*) as count from stream group by time(1m) having count > 0 limit 100 for background" },
                { "HavingClause","count > 0"},
                {"TimeGrouping", "1 minute"}
            }},
            {"Source", new Dictionary<string, object?>()
            {
                { "RangeStart", "2023-06-22T13:05:43.3585376Z"},
                { "RangeEnd", "2023-06-22T13:06:43.3585376Z"},
                {"ResultsUrl", "http://localhost:80/#/events?q=select%20count%28%2A%29%20as%20count%20from%20stream%20group%20by%20time%281m%29%20having%20count%20%3E%200%20limit%20100%20for%20background&from=2023-06-22T13:05:43.3585376Z&to=2023-06-22T13:06:43.3585376Z" },
                { "Results",  null },
                { "ContributingEvents",  null }
            }},
            { "SuppressedUntil", "2023-06-22T13:08:13.3585376Z"},
            { "Failures", null }
        };

        var data = new Dictionary<string, object?>()
        {
            { "Id", _event.Id },
            { "TimeStamp", _event.Timestamp.ToString("O") },
            { "Level", _event.Data.Level.ToString() },
            { "MessageTemplate", _event.Data.MessageTemplate },
            { "Message", _event.Data.RenderedMessage },
            { "Exception", _event.Data.Exception },
            { "Properties", props },
            { "EventType", 2716299265 },
            { "AppTitle", "app title" },
            { "InstanceName", "instance name" },
            { "BaseUri", "http://localhost" }
        };

        using var stream = typeof(TeamsApp).Assembly.GetManifestResourceStream("Seq.App.Teams.AdaptiveCard.Resources.default-template.json")!;
        using var reader = new StreamReader(stream);
        var template = reader.ReadToEnd();

        var cardTemplate = new AdaptiveCardTemplate(template);
        var result = cardTemplate.Expand(data);
        var errors = cardTemplate.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result.Count(c => c == '$'), Is.EqualTo(1));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }
}
