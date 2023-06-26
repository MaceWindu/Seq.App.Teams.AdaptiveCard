namespace Seq.App.Teams;

public sealed partial class TeamsApp
{
    private static void RegisterCustomfunctions()
    {
        // _nomd function escapes markdown control characters to disable markdown
        AdaptiveExpressions.Expression.Functions.Add("_nomd", (args) =>
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
                    .Replace("*", "\\*")
                    .Replace("-", "\\-")
                    .Replace(".", "\\.")
                    .Replace("[", "\\[")
                    ;
            }

            return input;
        });

        // _fixstr function adds missing escaping to string, embedded in json string
        // to workaround templating library bug
        //AdaptiveExpressions.Expression.Functions.Add("_fixstr", (args) =>
        //{
        //    var input = args[0];

        //    if (input is string strValue)
        //    {
        //    }

        //    return input;
        //});
    }
}
