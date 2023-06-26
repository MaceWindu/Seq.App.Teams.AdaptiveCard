using NUnit.Framework;
using Seq.App.Teams;

[SetUpFixture]
#pragma warning disable CA1050 // Declare types in namespaces
public sealed class TestsInitialization
#pragma warning restore CA1050 // Declare types in namespaces
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
