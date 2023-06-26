using Seq.Apps;
using Seq.Apps.LogEvents;
using Serilog;
using System;
using System.Collections.Generic;

namespace Seq.App.Teams;

[SeqApp("Teams (AdaptiveCard)", Description = "Sends events and notifications to Microsoft Teams.")]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public sealed partial class TeamsApp : SeqApp, ISubscribeToAsync<LogEventData>
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private ILogger _log = default!;
    private string _defaultTemplate = default!;
    private HashSet<LogEventLevel>? _loggedLevels;


    private HashSet<LogEventLevel> LogEventLevelList
    {
        get
        {
            if (_loggedLevels == null)
            {
                var result = new HashSet<LogEventLevel>();
                if (!string.IsNullOrEmpty(LogEventLevels))
                {

                    var strValues = LogEventLevels!.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strValues?.Length > 0)
                    {
                        foreach (var strValue in strValues)
                        {
                            if (Enum.TryParse(strValue, out LogEventLevel enumValue))
                            {
                                _ = result.Add(enumValue);
                            }
                        }
                    }
                }

                _loggedLevels = result;
            }

            return _loggedLevels;
        }
    }
}
