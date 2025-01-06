using AdaptiveCards.Templating;
using NUnit.Framework;
using System.Text.Json;

namespace Seq.App.Teams.Tests;

internal sealed class CustomFunctionsTests
{
    [TestCase(null, "\"${val}\"")]
    [TestCase(123, "123")]
    [TestCase("_italic_", "\"\\\\_italic\\\\_\"")]
    [TestCase("`magic`", "\"\\\\\\u0060magic\\\\\\u0060\"")]
    [TestCase("*bold too*", "\"\\\\*bold too\\\\*\"")]
    [TestCase("**bold**", "\"\\\\*\\\\*bold\\\\*\\\\*\"")]
    [TestCase("*italic too*", "\"\\\\*italic too\\\\*\"")]
    [TestCase("- list\r\n- item", "\"\\\\- list\\r\\n\\\\- item\"")]
    [TestCase("1. list\r\n2. item", "\"1\\\\. list\\r\\n2\\\\. item\"")]
    [TestCase("[link](http://local.host)", "\"[link\\\\](http://local.host)\"")]
    public void TestNoMarkdown(object? value, string expected)
    {
        var cardTemplate = new AdaptiveCardTemplate($"{{\"key\":\"${{_nomd(val)}}\"}}");
        var result = cardTemplate.Expand(new { val = value });
        var errors = cardTemplate.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result, Is.EqualTo($"{{\"key\":{expected}}}"));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [Test]
    public void TestJsonFormatting()
    {
        var cardTemplate = new AdaptiveCardTemplate($"{{\"key\":\"${{_jsonPrettify(val)}}\"}}");
        var result = cardTemplate.Expand(new { val = new { one = 1, two = "two", three = (string?)null } });
        var errors = cardTemplate.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result, Is.EqualTo(/*lang=json,strict*/ "{\"key\":\"{\\n\\n\\u2800\\u2800\\u0022one\\u0022: 1,\\n\\n\\u2800\\u2800\\u0022two\\u0022: \\u0022two\\u0022,\\n\\n\\u2800\\u2800\\u0022three\\u0022: null\\n\\n}\"}"));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }

    [TestCase("222222", "data:image/gif;base64,R0lGODlhAQABAIAAACIiIgAAACH5BAAAAAAALAAAAAABAAEAAAICRAEAOw==")]
    [TestCase("921b3c", "data:image/gif;base64,R0lGODlhAQABAIAAAJIbPAAAACH5BAAAAAAALAAAAAABAAEAAAICRAEAOw==")]
    [TestCase("921B3C", "data:image/gif;base64,R0lGODlhAQABAIAAAJIbPAAAACH5BAAAAAAALAAAAAABAAEAAAICRAEAOw==")]
    [TestCase("ffb748", "data:image/gif;base64,R0lGODlhAQABAIAAAP\\u002B3SAAAACH5BAAAAAAALAAAAAABAAEAAAICRAEAOw==")]
    [TestCase("016DA9", "data:image/gif;base64,R0lGODlhAQABAIAAAAFtqQAAACH5BAAAAAAALAAAAAABAAEAAAICRAEAOw==")]
    [TestCase("777777", "data:image/gif;base64,R0lGODlhAQABAIAAAHd3dwAAACH5BAAAAAAALAAAAAABAAEAAAICRAEAOw==")]
    [TestCase(null, "")]
    [TestCase(1, "")]
    [TestCase("123", "")]
    public void TestColorFunction(object? color, string expected)
    {
        var cardTemplate = new AdaptiveCardTemplate($"{{\"key\":\"${{_colorUri(color)}}\"}}");
        var result = cardTemplate.Expand(new { color });
        var errors = cardTemplate.GetLastTemplateExpansionWarnings();

        Assert.That(errors, Is.Empty);
        Assert.That(result, Is.EqualTo($"{{\"key\":\"{expected}\"}}"));
        Assert.DoesNotThrow(() => JsonDocument.Parse(result));
    }
}
