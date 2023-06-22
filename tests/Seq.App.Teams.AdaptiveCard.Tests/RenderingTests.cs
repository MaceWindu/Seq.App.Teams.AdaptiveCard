﻿using AdaptiveCards.Templating;
using NUnit.Framework;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;

namespace Seq.App.Teams.Tests;

public sealed class RenderingTests
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


    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${unknown_field}\"}", /*lang=json,strict*/ "{\"key\":\"${unknown_field}\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${null_field}\"}", /*lang=json,strict*/ "{\"key\":\"${null_field}\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${null_.field}\"}", /*lang=json,strict*/ "{\"key\":\"${null_.field}\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${unknown_.field}\"}", /*lang=json,strict*/ "{\"key\":\"${unknown_.field}\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Id}\"}", /*lang=json,strict*/ "{\"key\":\"event-id\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${EventType}\"}", /*lang=json,strict*/ "{\"key\":4294967295}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${TimestampUtc}\"}", /*lang=json,strict*/ "{\"key\":\"2020-01-22T18:19:59.1234567Z\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.Id}\"}", /*lang=json,strict*/ "{\"key\":\"some-other-id\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"$data\": \"${Data}\", \"key\": \"${Id}\"}", /*lang=json,strict*/ "{\"key\":\"some-other-id\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.LocalTimestamp}\"}", /*lang=json,strict*/ "{\"key\":\"2020-01-22T18:19:59.1234567+00:45\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.Level}\"}", /*lang=json,strict*/ "{\"key\":5}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.MessageTemplate}\"}", /*lang=json,strict*/ "{\"key\":\"template {message}\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.RenderedMessage}\"}", /*lang=json,strict*/ "{\"key\":\"rendered message\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.Exception}\"}", /*lang=json,strict*/ "{\"key\":\"exception\\r\\ndata\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.Properties.#FieldString}\"}", /*lang=json,strict*/ "{\"key\":\"my string\"}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.Properties.#FieldNumber}\"}", /*lang=json,strict*/ "{\"key\":-123}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.Properties.#FieldBoolean}\" }", /*lang=json,strict*/ "{\"key\":true}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.Properties.#FieldSimpleStringArray}\"}", /*lang=json,strict*/ "{\"key\":[\"1\",\"2\",\"3\"]}")]
    [TestCase(/*lang=json,strict*/ "{ \"key\": \"${Data.Properties.#FieldSimpleIntArray}\"}", /*lang=json,strict*/ "{\"key\":[1,2,3]}")]
    [TestCase(/*lang=json,strict*/ "[{ \"$data\": \"${Data.Properties.#FieldSimpleIntArray}\", \"value\": \"${$data}\" }]", /*lang=json,strict*/ "[{\"value\":1},{\"value\":2},{\"value\":3}]")]
    [TestCase(/*lang=json,strict*/ "[{ \"$data\": \"${Data.Properties.#FieldObjectArray}\", \"value\": \"${one}=>${two}\" }]", /*lang=json,strict*/ "[{\"value\":\"1=>2\"},{\"value\":\"-1=>-2\"}]")]
    public void TestRendering(string template, string expected)
    {
        var tmpl = new AdaptiveCardTemplate(template);
        var result = tmpl.Expand(_event);
        var errors = tmpl.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(null, /*lang=json,strict*/ "{\"key\":null}")]
    [TestCase(true, /*lang=json,strict*/ "{\"key\":true}")]
    [TestCase(123, /*lang=json,strict*/ "{\"key\":123}")]
    [TestCase("string", /*lang=json,strict*/ "{\"key\":\"string\"}")]
    public void TestRenderingUnknown(object? substitution, string expected)
    {
        var tmpl = new AdaptiveCardTemplate(/*lang=json,strict*/ "{\"key\":\"${smth}\"}");
        var result = tmpl.Expand(_event, _ => substitution);
        var errors = tmpl.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result, Is.EqualTo(expected));
    }
}