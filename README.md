# Seq.App.Teams.AdaptiveCard [![Build Status](https://img.shields.io/azure-devops/build/macewindu/projects/2/master?label=build%20(master))](https://dev.azure.com/macewindu/projects/_build/latest?definitionId=2&branchName=master) [![NuGet](https://img.shields.io/nuget/v/Seq.App.Teams.AdaptiveCard.svg)](https://www.nuget.org/packages/Seq.App.Teams.AdaptiveCard/) [![License](https://img.shields.io/github/license/MaceWindu/Seq.App.Teams.AdaptiveCard)](LICENSE.txt)

Seq alerting application for Microsoft Teams with AdaptiveCard support. Based on [Seq.App.Teams](https://github.com/AntoineGa/Seq.App.Teams) application.

## Notes

To connect application instance to Teams you need to provide Teams Webhook url. You can find required instructions [here](https://learn.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook?tabs=dotnet).

Because Teams currently doesn't support AdaptiveCard templating, it is done on application side using [AdaptiveCards.Templating](https://www.nuget.org/packages/AdaptiveCards.Templating) library.

## Context

Default template source: [default-template.json](https://github.com/MaceWindu/Seq.App.Teams.AdaptiveCard/blob/master/src/Seq.App.Teams.AdaptiveCard/Resources/default-template.json). Used when no template specified.

### Payload Examples

Those examples could be used with `AdaptiveCard` designer.

#### Event Payload

```json
{
    "Id": "event-4f3bb930732008db0733010000000000",
    "TimeStamp": "2023-06-22T12:58:00.8518960Z",
    "Level": "Warning",
    "MessageTemplate": "Some event {@data}",
    "Message": "Some event 123",
    "Exception": null,
    "Properties": {
        "#ProcessID": 11860,
        "#ThreadID": 88
    },
    "EventType": 2739387145,
    "AppTitle": "teams app",
    "InstanceName": "",
    "BaseUri": "http://localhost:80"
}
```

#### Alert Payload

```json
{
    "Id": "event-988d8be0732108db0000000000000000",
    "TimeStamp": "2023-06-22T13:07:13.3585376Z",
    "Level": "Warning",
    "MessageTemplate": "Alert condition triggered by {NamespacedAlertTitle}",
    "Message": "Alert condition triggered by \"admin/New Alert\"",
    "Exception": null,
    "Properties": {
        "NamespacedAlertTitle": "admin/New Alert",
        "Alert": {
            "Id": "alert-463",
            "Title": "New Alert",
            "Url": "http://localhost:80/#/alerts/alert-463",
            "OwnerUsername": "admin",
            "SignalExpressionDescription": null,
            "Query": "select count(*) as count from stream group by time(1m) having count > 0 limit 100 for background",
            "HavingClause": "count > 0",
            "TimeGrouping": "1 minute"
        },
        "Source": {
            "RangeStart": "2023-06-22T13:05:43.3585376Z",
            "RangeEnd": "2023-06-22T13:06:43.3585376Z",
            "ResultsUrl": "http://localhost:80/#/events?q=select%20count%28%2A%29%20as%20count%20from%20stream%20group%20by%20time%281m%29%20having%20count%20%3E%200%20limit%20100%20for%20background&from=2023-06-22T13:05:43.3585376Z&to=2023-06-22T13:06:43.3585376Z",
            "Results": [
                [ "time", "count" ],
                [ "2023-06-22T13:05:43.3585376Z", 19 ]
            ],
            "ContributingEvents": [
                [ "id", "timestamp", "message" ],
                [ "event-6d291458732108db9333010000000000", "2023-06-22T13:06:00.5580888Z", "event message 1" ],
                [ "event-6d291458732108db9433010000000000", "2023-06-22T13:06:00.5580888Z", "event message 2" ]
            ]
        },
        "SuppressedUntil": "2023-06-22T13:08:13.3585376Z",
        "Failures": null
    },
    "EventType": 2716299265,
    "AppTitle": "teams app",
    "InstanceName": "",
    "BaseUri": "http://localhost:80"
}
```

### Custom Functions

In addition to standard AdaptiveTemplate [functions](https://learn.microsoft.com/en-us/azure/bot-service/adaptive-expressions/adaptive-expressions-prebuilt-functions?view=azure-bot-service-4.0), application implements several custom functions:

- `_nomd(param)`: this function takes single parameter of any type and in case of string escapes all supported [markdown control sequences](https://learn.microsoft.com/en-us/adaptive-cards/authoring-cards/text-features). Function is useful when rendered text could contain markdown-like character sequences as AdaptiveCard doesn't provide option to disable markdown from template.

## Extra Links

### Configuration

- [Teams: Create Incoming Webhooks](https://learn.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/add-incoming-webhook?tabs=dotnet). Provides instructions on Teams webhook creation

### Dependencies

- [AdaptiveCards.Templating repository](https://github.com/microsoft/AdaptiveCards). Use it for issue reports requests for templating engine
- [AdaptiveExpressions repository](https://github.com/Microsoft/botbuilder-dotnet). Contains templating expressions evaluation engine sources

### Templating language

- [AdaptiveCard visual designer](https://adaptivecards.io/designer/)
- [Templating language reference](https://learn.microsoft.com/en-us/adaptive-cards/templating/language)
- [List of built-in functions](https://learn.microsoft.com/en-us/azure/bot-service/adaptive-expressions/adaptive-expressions-prebuilt-functions?view=azure-bot-service-4.0)
- [Client-side templating](https://learn.microsoft.com/en-us/adaptive-cards/authoring-cards/text-features). Additional templating features (those are supported by Teams)
- [Teams extensions](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-format?tabs=adaptive-md%2Cdesktop%2Cconnector-html). Teams-specific AdaptiveCard extensions
