# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A [Seq](https://datalust.co/seq) alerting app (NuGet package `Seq.App.Teams.AdaptiveCard`) that posts events/alerts to Microsoft Teams as AdaptiveCards. Loaded into a Seq server, configured via app settings. Because Teams does not template AdaptiveCards server-side, templating is done in-process via `AdaptiveCards.Templating`.

## Commands

```powershell
dotnet build Seq.App.Teams.AdaptiveCard.sln
dotnet test Seq.App.Teams.AdaptiveCard.sln          # all tests (NUnit)
dotnet test --filter "FullyQualifiedName~RenderingTests"   # single fixture
dotnet test --filter "Name~TestRendering"                  # single test
```

Build is strict: `TreatWarningsAsErrors=true`, `WarningLevel 9999`, `AnalysisLevel=preview-All`, nullable enabled. Analyzer/style violations fail the build — fix them, don't suppress unless following the existing `#pragma warning disable CA####` pattern.

## Layout & versioning

- `src/Seq.App.Teams.AdaptiveCard/` — the app, targets **netstandard2.0** (Seq plugin requirement; `PolySharp` backfills newer language features). One `TeamsApp` class split across partial files: `.cs` (settings parsing), `.Settings.cs` (`SeqAppSetting` UI fields), `.OnAttached.cs` (init), `.OnEvent.cs` (payload + send), `.CustomFunctions.cs` (template functions).
- `tests/` — targets net8.0, NUnit.
- Central package versions in `Directory.Packages.props`; shared build config in `Directory.Build.props`.
- Version numbers live in **`ci/default.yml`** (`assemblyVersion`/`packageVersion`), not the csproj — CI rewrites the csproj via `ci/SetVersion.ps1`. NuGet packaging is driven by `ci/Seq.App.Teams.AdaptiveCard.nuspec`, not `dotnet pack`. CI is Azure DevOps (`ci/default.yml`).

## Architecture notes

- **Entry point**: `TeamsApp` implements `ISubscribeToAsync<LogEventData>`; `OnAsync` (in `.OnEvent.cs`) is invoked per event. It filters by `LogEventLevels`, builds a flat payload dict (`BuildPayload`), expands the template (user-supplied `CardTemplate` or the embedded `Resources/default-template.json`), wraps it in the Teams `attachments` envelope, and POSTs to the webhook.
- The **default template is an embedded resource** loaded in `OnAttached`. Changing default rendering = editing `Resources/default-template.json`.
- **Custom AdaptiveExpressions functions** (`_nomd`, `_jsonPrettify`, `_colorUri`) are registered globally via `Expression.Functions.Add` in `RegisterCustomFunctions()`. This is process-global static state — tests call it once in `Init.cs` `[OneTimeSetUp]`. See README "Custom Functions" for semantics.
- **`PropertiesToExclude`** uses a hand-written parser (`ParsePropertyPath` in `TeamsApp.cs`) for the `[name][nested]` path syntax with `\` escaping; `BuildPayload` walks these paths to delete properties from the model before expansion.
- Target **AdaptiveCard schema version 1.3** for templates — Power Automate workflow integration caps at 1.3 even though Teams supports 1.5.

## Reference

The README is the authoritative spec for end users: webhook vs. Workflow setup, payload shapes (Event vs. Alert), and custom function behavior. Consult it before changing payload fields or template semantics.
