using NUnit.Framework;
using Seq.App.Teams;
using System.Diagnostics.CodeAnalysis;

// Intentionally declared in the global namespace: a [SetUpFixture] in the global namespace applies to
// the whole test assembly, whereas a namespaced one only covers its own namespace and sub-namespaces.
[SetUpFixture]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable MA0047 // Declare types in namespaces
internal sealed class TestsInitialization
#pragma warning restore MA0047 // Declare types in namespaces
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
