using NUnit.Framework;

namespace Seq.App.Teams.Tests;

public sealed class SettingsTests
{
    [TestCase("", null)]
    [TestCase(" ", null)]
    [TestCase("]", null)]
    [TestCase("[]]", null)]
    [TestCase("[\r]", null)]
    [TestCase("[\n]", null)]
    [TestCase("[\\ ]", null)]
    [TestCase("[]", new[] { "" })]
    [TestCase("  []  ", new[] { "" })]
    [TestCase("  [][]  ", new[] { "", "" })]
    [TestCase("[Properties][LogName]", new[] { "Properties", "LogName" })]
    [TestCase("[Prope\\\\rties]", new[] { "Prope\\rties" })]
    [TestCase("[Prope\\rrties]", new[] { "Prope\rrties" })]
    [TestCase("[Prope\\nrties]", new[] { "Prope\nrties" })]
    [TestCase("[Prope\\]rties]", new[] { "Prope]rties" })]
    [TestCase("[Prope[rties]", new[] { "Prope[rties" })]
    public void TestJsonPathParser(string jsonPath, string[]? expected)
    {
        var result = TeamsApp.ParsePropertyPath(jsonPath);

        Assert.That(result, Is.EqualTo(expected));
    }
}
