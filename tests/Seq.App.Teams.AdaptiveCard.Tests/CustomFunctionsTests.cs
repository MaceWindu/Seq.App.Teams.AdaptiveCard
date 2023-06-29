﻿using AdaptiveCards.Templating;
using NUnit.Framework;
using System.Text.Json;

namespace Seq.App.Teams.Tests;

public sealed class CustomFunctionsTests
{
    [TestCase(null, "\"${val}\"")]
    [TestCase(123, "123")]
    [TestCase("_italic_", "\"\\\\_italic\\\\_\"")]
    [TestCase("**bold**", "\"\\\\**bold\\\\**\"")]
    [TestCase("- list\r\n- item", "\"\\\\- list\\r\\n\\\\- item\"")]
    [TestCase("1. list\r\n2. item", "\"1\\\\. list\\r\\n2\\\\. item\"")]
    [TestCase("[link](http://local.host)", "\"[link\\\\](http://local.host)\"")]
    public void TestNoMarkdown(object? value, string expected)
    {
        var tmpl = new AdaptiveCardTemplate($"{{\"key\":\"${{_nomd(val)}}\"}}");
        var result = tmpl.Expand(new { val = value });
        var errors = tmpl.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result, Is.EqualTo($"{{\"key\":{expected}}}"));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [Test]
    public void TestJsonFormatting()
    {
        var tmpl = new AdaptiveCardTemplate($"{{\"key\":\"${{_jsonPrettify(val)}}\"}}");
        var result = tmpl.Expand(new { val = new { one = 1, two = "two", three = (string?)null } });
        var errors = tmpl.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result, Is.EqualTo(/*lang=json,strict*/ "{\"key\":\"{\\n\\n⠀⠀\\\"one\\\": 1,\\n\\n⠀⠀\\\"two\\\": \\\"two\\\",\\n\\n⠀⠀\\\"three\\\": null\\n\\n}\"}"));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [TestCase("222222", "data:image/bmp;base64,Qk06AAAAAAAAADYAAAAoAAAAAQAAAAEAAAABABgAAAAAAAQAAADDDgAAww4AAAAAAAAAAAAAIiIiAA==")]
    [TestCase("921b3c", "data:image/bmp;base64,Qk06AAAAAAAAADYAAAAoAAAAAQAAAAEAAAABABgAAAAAAAQAAADDDgAAww4AAAAAAAAAAAAAPBuSAA==")]
    [TestCase("921B3C", "data:image/bmp;base64,Qk06AAAAAAAAADYAAAAoAAAAAQAAAAEAAAABABgAAAAAAAQAAADDDgAAww4AAAAAAAAAAAAAPBuSAA==")]
    [TestCase("ffb748", "data:image/bmp;base64,Qk06AAAAAAAAADYAAAAoAAAAAQAAAAEAAAABABgAAAAAAAQAAADDDgAAww4AAAAAAAAAAAAASLf/AA==")]
    [TestCase("016DA9", "data:image/bmp;base64,Qk06AAAAAAAAADYAAAAoAAAAAQAAAAEAAAABABgAAAAAAAQAAADDDgAAww4AAAAAAAAAAAAAqW0BAA==")]
    [TestCase("777777", "data:image/bmp;base64,Qk06AAAAAAAAADYAAAAoAAAAAQAAAAEAAAABABgAAAAAAAQAAADDDgAAww4AAAAAAAAAAAAAd3d3AA==")]
    [TestCase(null, "")]
    [TestCase(1, "")]
    [TestCase("123", "")]
    public void TestColorFunction(object? color, string expected)
    {
        var tmpl = new AdaptiveCardTemplate($"{{\"key\":\"${{_colorUri(color)}}\"}}");
        var result = tmpl.Expand(new { color });
        var errors = tmpl.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result, Is.EqualTo($"{{\"key\":\"{expected}\"}}"));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }
}
