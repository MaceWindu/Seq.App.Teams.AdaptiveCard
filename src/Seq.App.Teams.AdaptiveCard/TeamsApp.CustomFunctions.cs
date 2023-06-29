using Newtonsoft.Json;

namespace Seq.App.Teams;

public sealed partial class TeamsApp
{
    private static readonly JsonSerializerSettings _prettyPrint = new()
    {
        Formatting = Formatting.Indented,
        MaxDepth = null,
        NullValueHandling = NullValueHandling.Ignore
    };

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
            return JsonConvert.SerializeObject(args[0], _prettyPrint);
        });
    }
}
