using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace Seq.App.Teams.Tests;

[SetUpFixture]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal sealed class TestsInitialization
{
    [OneTimeSetUp]
    public void TestAssemblySetup()
    {
        TeamsApp.RegisterCustomFunctions();
    }

    [OneTimeTearDown]
    public void TestAssemblyTeardown()
    {
    }
}
