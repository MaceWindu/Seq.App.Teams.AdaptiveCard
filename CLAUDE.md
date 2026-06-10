# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A [Seq](https://datalust.co/seq) alerting app (NuGet package `Seq.App.Teams.AdaptiveCard`) that posts events/alerts to Microsoft Teams as AdaptiveCards. Loaded into a Seq server, configured via app settings. Because Teams does not template AdaptiveCards server-side, templating is done in-process via `AdaptiveCards.Templating`.

## Commands

Requires the .NET 10 SDK (pinned in `global.json`). Solution is `Seq.App.Teams.AdaptiveCard.slnx`.

```powershell
dotnet build Seq.App.Teams.AdaptiveCard.slnx -c Release
dotnet test  Seq.App.Teams.AdaptiveCard.slnx -c Release   # all tests (NUnit on Microsoft.Testing.Platform)
# single test/fixture — filter expression goes after the `--` separator:
dotnet test Seq.App.Teams.AdaptiveCard.slnx -- --filter "FullyQualifiedName~RenderingTests"
dotnet pack  Seq.App.Teams.AdaptiveCard.slnx -c Release    # produces the NuGet package (see Packaging)
```

Tests run on **Microsoft.Testing.Platform** (not VSTest/`Microsoft.NET.Test.Sdk`) — selected via `global.json`'s `test.runner` and `EnableNUnitRunner` in the test csproj. Trx output: `dotnet test … -- --report-trx`.

Build is strict: `TreatWarningsAsErrors=true`, `WarningLevel 9999`, `AnalysisLevel=preview-All`, nullable enabled, plus **AsyncFixer** and **Meziantou.Analyzer** (all rules on). Violations fail the build — fix them, or disable a specific rule in `.editorconfig` with a reason (see the existing MA0016/MA0101 entries) when it's a deliberate false positive.

## Layout & versioning

- `src/Seq.App.Teams.AdaptiveCard/` — the app, targets **netstandard2.0** (Seq plugin requirement; **Meziantou.Polyfill** backfills the few newer BCL types/methods used — the enabled set is listed explicitly in the csproj `Enabled Polyfills` group). One `TeamsApp` class split across partial files: `.cs` (settings parsing + `ParsePropertyPath`), `.Settings.cs` (`SeqAppSetting` UI fields), `.OnAttached.cs` (init), `.OnEvent.cs` (payload + send), `.CustomFunctions.cs` (template functions).
- `tests/` — targets **net10.0**, NUnit on Microsoft.Testing.Platform. `TestsInitialization.cs` is the assembly `[SetUpFixture]`.
- Central package versions in `Directory.Packages.props`; shared build config in `Directory.Build.props`.
- **Version lives in `Directory.Build.props`** (`<Version>`); bump it after each release. Packages get a prerelease suffix via `VersionSuffix`/`ApplyVersionSuffix` (CI passes `-p:VersionSuffix=-rc.<build>` for dev feeds, `-p:ApplyVersionSuffix=false` on the `release` branch). CI is Azure DevOps (`ci/default.yml`).

## Packaging

`dotnet pack` produces the package; all metadata is in the csproj `Package` group (no `.nuspec`). The Seq host does **not** resolve NuGet dependencies, so the plugin's dependency DLLs are **bundled** into `lib/netstandard2.0/` by the `IncludeDependencyDlls` target (with `SuppressDependenciesWhenPacking`, so no `<dependencies>` are declared). The target bundles every copy-local DLL **except** the assemblies the Seq host already provides (denylist `_HostProvidedDlls`: Seq.Apps, Serilog, Newtonsoft.Json, System.Text.Json + BCL shims). If AdaptiveCards.Templating pulls a new transitive dep, it's bundled automatically; if you reference a package the host already provides, add it to the denylist. Symbols are embedded in the assembly (`DebugType=embedded` + SourceLink + DotNet.ReproducibleBuilds) — no `.snupkg`.

## Architecture notes

- **Entry point**: `TeamsApp` implements `ISubscribeToAsync<LogEventData>`; `OnAsync` (in `.OnEvent.cs`) is invoked per event. It filters by `LogEventLevels`, builds a flat payload dict (`BuildPayload`), expands the template (user-supplied `CardTemplate` or the embedded `Resources/default-template.json`), wraps it in the Teams `attachments` envelope, and POSTs to the webhook.
- The **default template is an embedded resource** loaded in `OnAttached`. Changing default rendering = editing `Resources/default-template.json`.
- **Custom AdaptiveExpressions functions** (`_nomd`, `_jsonPrettify`, `_colorUri`) are registered globally via `Expression.Functions.Add` in `RegisterCustomFunctions()` (each backed by a private method `NoMarkdown`/`JsonPrettify`/`ColorUri`). This is process-global static state — tests call it once in `TestsInitialization.cs` `[OneTimeSetUp]`. See README "Custom Functions" for semantics.
- **`PropertiesToExclude`** uses a hand-written parser (`ParsePropertyPath` in `TeamsApp.cs`) for the `[name][nested]` path syntax with `\` escaping; `BuildPayload` walks these paths to delete properties from the model before expansion.
- Target **AdaptiveCard schema version 1.3** for templates — Power Automate workflow integration caps at 1.3 even though Teams supports 1.5.

## Reference

The README is the authoritative spec for end users: webhook vs. Workflow setup, payload shapes (Event vs. Alert), and custom function behavior. Consult it before changing payload fields or template semantics.
