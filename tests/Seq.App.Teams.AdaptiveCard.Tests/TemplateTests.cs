using AdaptiveCards.Templating;
using NUnit.Framework;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        Assert.That(result.Count(c => c == '$'), Is.EqualTo(1));
        Assert.That(result, Is.EqualTo(/*lang=json,strict*/ @"{""type"":""AdaptiveCard"",""$schema"":""http://adaptivecards.io/schemas/adaptive-card.json"",""version"":""1.4"",""body"":[{""type"":""Container"",""backgroundImage"":{""url"":""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAMSURBVBhXY1BSUgIAANAAZ+lxJ0oAAAAASUVORK5CYII=""},""items"":[{""type"":""TextBlock"",""text"":""**Fatal**"",""wrap"":true,""size"":""ExtraLarge"",""color"":""light""}]},{""type"":""TextBlock"",""text"":""{{DATE(2020-01-22T18:19:59Z,SHORT)}} {{TIME(2020-01-22T18:19:59Z)}}"",""wrap"":true,""color"":""Attention""},{""type"":""Container"",""items"":[{""type"":""TextBlock"",""text"":""**rendered message**"",""wrap"":true},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldString**"",""value"":""my string""}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldNumber**"",""value"":-123}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldBoolean**"",""value"":true}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldNull**"",""value"":""""}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldSimpleStringArray**"",""value"":[""1"",""2"",""3""]}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldSimpleIntArray**"",""value"":[1,2,3]}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldObjectArray**"",""value"":[{""one"":1,""two"":2},{""one"":-1,""two"":-2}]}]},{""type"":""TextBlock"",""text"":""exception\r\ndata""}]},{""type"":""ColumnSet"",""columns"":[{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""ActionSet"",""actions"":[{""type"":""Action.OpenUrl"",""title"":""View Event in Seq"",""url"":""http://localhost#/events?filter=@Id%3D'event-id'&show=expanded""}]}]},{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""ActionSet"",""actions"":[{""type"":""Action.OpenUrl"",""title"":""Open Seq"",""url"":""http://localhost""}],""horizontalAlignment"":""Right""}]}]}]}"));
    }

    [Test]
    public void TestDefaultTemplateNoOptionsData()
    {
        var data = new Payload(
            _event.Id,
            _event.TimestampUtc.ToString("O"),
            _event.Data.Level.ToString(),
            _event.Data.MessageTemplate,
            _event.Data.RenderedMessage,
            null,
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
        Assert.That(result.Count(c => c == '$'), Is.EqualTo(1));
        Assert.That(result, Is.EqualTo(/*lang=json,strict*/ @"{""type"":""AdaptiveCard"",""$schema"":""http://adaptivecards.io/schemas/adaptive-card.json"",""version"":""1.4"",""body"":[{""type"":""Container"",""backgroundImage"":{""url"":""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAMSURBVBhXY1BSUgIAANAAZ+lxJ0oAAAAASUVORK5CYII=""},""items"":[{""type"":""TextBlock"",""text"":""**Fatal**"",""wrap"":true,""size"":""ExtraLarge"",""color"":""light""}]},{""type"":""TextBlock"",""text"":""{{DATE(2020-01-22T18:19:59Z,SHORT)}} {{TIME(2020-01-22T18:19:59Z)}}"",""wrap"":true,""color"":""Attention""},{""type"":""Container"",""items"":[{""type"":""TextBlock"",""text"":""**rendered message**"",""wrap"":true},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldString**"",""value"":""my string""}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldNumber**"",""value"":-123}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldBoolean**"",""value"":true}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldNull**"",""value"":""""}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldSimpleStringArray**"",""value"":[""1"",""2"",""3""]}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldSimpleIntArray**"",""value"":[1,2,3]}]},{""type"":""FactSet"",""facts"":[{""title"":""**#FieldObjectArray**"",""value"":[{""one"":1,""two"":2},{""one"":-1,""two"":-2}]}]}]},{""type"":""ColumnSet"",""columns"":[{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""ActionSet"",""actions"":[{""type"":""Action.OpenUrl"",""title"":""View Event in Seq"",""url"":""http://localhost#/events?filter=@Id%3D'event-id'&show=expanded""}]}]},{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""ActionSet"",""actions"":[{""type"":""Action.OpenUrl"",""title"":""Open Seq"",""url"":""http://localhost""}],""horizontalAlignment"":""Right""}]}]}]}"));
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
        var data = new Payload(
            _event.Id,
            _event.TimestampUtc.ToString("O"),
            _event.Data.Level.ToString(),
            _event.Data.MessageTemplate,
            _event.Data.RenderedMessage,
            _event.Data.Exception,
            props,
            2716299265,
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
        Assert.That(result.Count(c => c == '$'), Is.EqualTo(1));
        Assert.That(result, Is.EqualTo(/*lang=json,strict*/ @"{""type"":""AdaptiveCard"",""$schema"":""http://adaptivecards.io/schemas/adaptive-card.json"",""version"":""1.4"",""body"":[{""type"":""Container"",""backgroundImage"":{""url"":""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAMSURBVBhXY1BSUgIAANAAZ+lxJ0oAAAAASUVORK5CYII=""},""items"":[{""type"":""TextBlock"",""text"":""**Fatal**"",""wrap"":true,""size"":""ExtraLarge"",""color"":""light""}]},{""type"":""TextBlock"",""text"":""{{DATE(2020-01-22T18:19:59Z,SHORT)}} {{TIME(2020-01-22T18:19:59Z)}}"",""wrap"":true,""color"":""Attention""},{""type"":""Container"",""items"":[{""type"":""TextBlock"",""text"":""Alert condition triggered by [Alert/Name](Alert.Url)"",""wrap"":true},{""type"":""Container"",""items"":[{""type"":""TextBlock"",""text"":""**Results**"",""wrap"":true},{""type"":""ColumnSet"",""columns"":[{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":""**time**"",""wrap"":true}]},{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":""**count**"",""wrap"":true}]}]},{""type"":""ColumnSet"",""columns"":[{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":[""2023-06-22T13:05:43.3585376Z"",19],""wrap"":true}]},{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":[""2023-06-22T13:05:44.3585376Z"",5],""wrap"":true}]}]},{""type"":""TextBlock"",""text"":""[Explore detected results in Seq](http://localhost:80/#/events?q=select%20count%28%2A%29%20as%20count%20from%20stream%20group%20by%20time%281m%29%20having%20count%20%3E%200%20limit%20100%20for%20background&from=2023-06-22T13:05:43.3585376Z&to=2023-06-22T13:06:43.3585376Z)"",""wrap"":true}]},{""type"":""Container"",""items"":[{""type"":""TextBlock"",""text"":""**Contributing events**"",""wrap"":true},{""type"":""ColumnSet"",""columns"":[{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":""**Time**"",""wrap"":true}]},{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":""**Message**"",""wrap"":true}]}]},{""type"":""ColumnSet"",""columns"":[{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":""[2023-06-22T13:06:00.5580888Z](http://localhost#/events?filter=@Id%3D'event-6d291458732108db9333010000000000'&show=expanded)"",""wrap"":true}]},{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":""event message 1"",""wrap"":true}]}]},{""type"":""ColumnSet"",""columns"":[{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":""[2023-06-22T13:06:00.5580888Z](http://localhost#/events?filter=@Id%3D'event-6d291458732108db9433010000000000'&show=expanded)"",""wrap"":true}]},{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""TextBlock"",""text"":""event message 2"",""wrap"":true}]}]}]}]},{""type"":""ColumnSet"",""columns"":[{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""ActionSet"",""actions"":[]}]},{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""ActionSet"",""actions"":[{""type"":""Action.OpenUrl"",""title"":""Open Seq"",""url"":""http://localhost""}],""horizontalAlignment"":""Right""}]}]}]}"));
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
        var data = new Payload(
            _event.Id,
            _event.TimestampUtc.ToString("O"),
            _event.Data.Level.ToString(),
            _event.Data.MessageTemplate,
            _event.Data.RenderedMessage,
            _event.Data.Exception,
            props,
            2716299265,
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
        Assert.That(result.Count(c => c == '$'), Is.EqualTo(1));
        Assert.That(result, Is.EqualTo(/*lang=json,strict*/ @"{""type"":""AdaptiveCard"",""$schema"":""http://adaptivecards.io/schemas/adaptive-card.json"",""version"":""1.4"",""body"":[{""type"":""Container"",""backgroundImage"":{""url"":""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAMSURBVBhXY1BSUgIAANAAZ+lxJ0oAAAAASUVORK5CYII=""},""items"":[{""type"":""TextBlock"",""text"":""**Fatal**"",""wrap"":true,""size"":""ExtraLarge"",""color"":""light""}]},{""type"":""TextBlock"",""text"":""{{DATE(2020-01-22T18:19:59Z,SHORT)}} {{TIME(2020-01-22T18:19:59Z)}}"",""wrap"":true,""color"":""Attention""},{""type"":""Container"",""items"":[{""type"":""TextBlock"",""text"":""Alert condition triggered by [Alert/Name](Alert.Url)"",""wrap"":true}]},{""type"":""ColumnSet"",""columns"":[{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""ActionSet"",""actions"":[]}]},{""type"":""Column"",""width"":""stretch"",""items"":[{""type"":""ActionSet"",""actions"":[{""type"":""Action.OpenUrl"",""title"":""Open Seq"",""url"":""http://localhost""}],""horizontalAlignment"":""Right""}]}]}]}"));
    }

    [Test, Explicit]
    public void Debug()
    {
        //var data = @"TODO";
        var data = @"TODO";

        using var stream = typeof(TeamsApp).Assembly.GetManifestResourceStream("Seq.App.Teams.AdaptiveCard.Resources.default-template.json")!;
        using var reader = new StreamReader(stream);
        var template = reader.ReadToEnd();

        var tmpl = new AdaptiveCardTemplate(template);
        var result = tmpl.Expand(data);
        var errors = tmpl.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result.Count(c => c == '$'), Is.EqualTo(1));
    }
}
