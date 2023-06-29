using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace Seq.App.Teams;

public sealed partial class TeamsApp
{
    private static readonly Dictionary<(int, int, int), string> _colors = new();

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

        AdaptiveExpressions.Expression.Functions.Add("_colorUri", args =>
        {
            if (args[0] is string colorString
                && colorString.Length == 6
                && int.TryParse(colorString[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var red)
                && int.TryParse(colorString[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var green)
                && int.TryParse(colorString[4..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var blue))
            {
                var key = (red, green, blue);

                if (!_colors.TryGetValue(key, out var uri))
                {
                    using var bmp = new Bitmap(1, 1);
                    bmp.SetPixel(0, 0, Color.FromArgb(red, green, blue));

                    using var stream = new MemoryStream();
                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    var data = stream.ToArray();

                    uri = $"data:image/png;base64,{Convert.ToBase64String(data)}";
                    _colors[key] = uri;
                }

                return uri;
            }

            return string.Empty;
        });
    }
}
