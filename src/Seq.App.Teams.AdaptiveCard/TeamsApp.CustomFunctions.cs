using Newtonsoft.Json;
using System.IO;

namespace Seq.App.Teams;

public sealed partial class TeamsApp
{
    public static void RegisterCustomFunctions()
    {
        // _nomd function escapes markdown control characters to disable markdown
        AdaptiveExpressions.Expression.Functions.Add("_nomd", args =>
        {
            var input = args[0];

            if (input is string strValue)
            {
                // supported markdown controls
                // https://learn.microsoft.com/en-us/adaptive-cards/authoring-cards/text-features
                // "_", "**", "- ", "\D. ", "[?](?)"
                // there is no need to analyze text for real markdown sequences - we can just replace
                // character, used in markdown
                return strValue
                    .Replace("_", "\\_")
                    .Replace("**", "\\**")
                    .Replace("- ", "\\- ")
                    .Replace(". ", "\\. ")
                    .Replace("](", "\\](")
                    ;
            }

            return input;
        });

        AdaptiveExpressions.Expression.Functions.Add("_jsonPrettify", args =>
        {
            using var sw = new StringWriter();

            // Teams newline
            sw.NewLine = "\n\n";

            using (var jtw = new JsonTextWriter(sw))
            {
                jtw.Formatting = Formatting.Indented;

                // emulate whitespace with non-trimmed invisible character
                // (U+2800 BRAILLE PATTERN BLANK)
                jtw.IndentChar = '⠀';

                var serializer = new JsonSerializer()
                {
                    MaxDepth = null,
                    NullValueHandling = NullValueHandling.Ignore
                };

                serializer.Serialize(jtw, args[0]);
            }

            sw.Flush();
            return sw.ToString();
        });
    }
}
