# Seq.App.Teams.AdaptiveCard [![Build Status](https://img.shields.io/azure-devops/build/macewindu/projects/2/master?label=build%20(master))](https://dev.azure.com/macewindu/projects/_build/latest?definitionId=2&branchName=master) [![NuGet](https://img.shields.io/nuget/v/Seq.App.Teams.AdaptiveCard.svg)](https://www.nuget.org/packages/Seq.App.Teams.AdaptiveCard/) [![License](https://img.shields.io/github/license/MaceWindu/Seq.App.Teams.AdaptiveCard)](LICENSE.txt)

Seq alerting application for Microsoft Teams with AdaptiveCard support. Based on [Seq.App.Teams](https://github.com/AntoineGa/Seq.App.Teams) application.

## Notes

To connect application instance to Teams you need to provide Teams Webhook url. You can find required instructions [here](https://learn.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook?tabs=dotnet).

Because Teams currently doesn't support AdaptiveCard templating, it is done on application side using [AdaptiveCards.Templating](https://www.nuget.org/packages/AdaptiveCards.Templating) library.

## Context

Context type has [Event](https://github.com/datalust/seq-apps-runtime/blob/dev/src/Seq.Apps/Apps/Event.cs)`<`[LogEventData](https://github.com/datalust/seq-apps-runtime/blob/dev/src/Seq.Apps/Apps/LogEvents/LogEventData.cs)`>` type and it's properties could be referenced from template using their names:

```
${Id} -> event.Id
${Data.MessageTemplate} -> event.Data.MessageTemplate
${Data.Properties.SomeCustomProperty} -> event.Data.Properties["SomeCustomProperty"]
```

## Extra Links

### Configuration

- [Teams: Create Incoming Webhooks](https://learn.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook?tabs=dotnet). Provides instructions on Teams webhook creation

### Dependencies

- [AdaptiveCards.Templating repository](https://github.com/microsoft/AdaptiveCards). Use it for issue reports requests for templating engine
- [AdaptiveExpressions repository](https://github.com/Microsoft/botbuilder-dotnet). Contains templating expressions evaluation engine sources

### Templating language

- [AdaptiveCard visual designer](https://adaptivecards.io/designer/). Note that `AdaptiveCards.Templating` library currently supports only `AdaptiveCard` 1.4
- [Templating language reference](https://learn.microsoft.com/en-us/adaptive-cards/templating/language)
- [List of built-in functions](https://learn.microsoft.com/en-us/azure/bot-service/adaptive-expressions/adaptive-expressions-prebuilt-functions?view=azure-bot-service-4.0#formatEpoch)
- [Client-side templating](https://learn.microsoft.com/en-us/adaptive-cards/authoring-cards/text-features). Additional templating features (those are supported by Teams)
