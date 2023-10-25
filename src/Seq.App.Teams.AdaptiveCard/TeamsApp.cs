using Seq.Apps;
using Seq.Apps.LogEvents;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seq.App.Teams;

[SeqApp("Teams (AdaptiveCard)", Description = "Sends events and notifications to Microsoft Teams.")]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public sealed partial class TeamsApp : SeqApp, ISubscribeToAsync<LogEventData>
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private static readonly char[] _logLevelsSeparator = [','];
    private static readonly char[] _propertiesSeparators = ['\r', '\n'];

    private ILogger _log = default!;
    private string _defaultTemplate = default!;
    private HashSet<LogEventLevel>? _loggedLevels;
    private IReadOnlyList<IReadOnlyList<string>>? _excludedProperties;

    private HashSet<LogEventLevel> LogEventLevelList
    {
        get
        {
            if (_loggedLevels == null)
            {
                var result = new HashSet<LogEventLevel>();
                if (!string.IsNullOrEmpty(LogEventLevels))
                {
                    var strValues = LogEventLevels!.Split(_logLevelsSeparator, StringSplitOptions.RemoveEmptyEntries);
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

    private IReadOnlyList<IReadOnlyList<string>> ExcludedProperties
    {
        get
        {
            if (_excludedProperties == null)
            {
                var result = new List<IReadOnlyList<string>>();
                if (!string.IsNullOrWhiteSpace(PropertiesToExclude))
                {

                    var lines = PropertiesToExclude!.Split(_propertiesSeparators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines)
                    {
                        var propertyPath = ParsePropertyPath(line.Trim());
                        if (propertyPath != null)
                        {
                            result.Add(propertyPath);
                        }
                    }
                }

                _excludedProperties = result;
            }

            return _excludedProperties;
        }
    }

    public static List<string>? ParsePropertyPath(string path)
    {
        path = path.Trim();

        if (string.IsNullOrWhiteSpace(path) || path.Length < 2 || path[0] != '[' || path[^1] != ']')
        {
            return null;
        }

        List<string>? properties = null;

        var isInName = true;
        var valid = true;
        var sb = new StringBuilder();
        for (var i = 1; i < path.Length && valid; i++)
        {
            switch (path[i])
            {
                case ']':
                    if (isInName)
                    {
                        (properties ??= []).Add(sb.ToString());
                        sb.Length = 0;
                        isInName = false;
                    }
                    else
                    {
                        valid = false;
                    }
                    break;
                case '[':
                    if (isInName)
                    {
                        _ = sb.Append('[');
                    }
                    else
                    {
                        isInName = true;
                    }
                    break;
                case '\\':
                    if (path.Length < i + 2)
                    {
                        valid = false;
                    }
                    else
                    {
                        switch (path[i + 1])
                        {
                            case 'r':
                                _ = sb.Append('\r');
                                i++;
                                break;
                            case 'n':
                                _ = sb.Append('\n');
                                i++;
                                break;
                            case ']':
                            case '\\':
                                _ = sb.Append(path[i + 1]);
                                i++;
                                break;
                            default:
                                valid = false;
                                break;
                        }
                    }
                    break;
                case '\r':
                case '\n':
                    valid = false;
                    break;
                default:
                    if (isInName)
                    {
                        _ = sb.Append(path[i]);
                    }
                    else
                    {
                        valid = false;
                    }
                    break;
            }
        }

        return valid ? properties : null;
    }
}
