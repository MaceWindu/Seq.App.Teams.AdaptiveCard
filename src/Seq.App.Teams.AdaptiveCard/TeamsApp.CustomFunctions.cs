using Microsoft.Bot.AdaptiveExpressions.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json.Nodes;

namespace Seq.App.Teams;

public sealed partial class TeamsApp
{
    private static readonly Dictionary<(int, int, int), string> _colors = [];

    public static void RegisterCustomFunctions()
    {
        // _nomd function escapes markdown control characters to disable markdown
        Expression.Functions.Add("_nomd", args =>
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
                    // escape * instead of ** as MS implementation works not like documented
                    //.Replace("**", "\\**")
                    .Replace("*", "\\*")
                    .Replace("- ", "\\- ")
                    // for D. we should escape dot
                    .Replace(". ", "\\. ")
                    .Replace("](", "\\](")
                    // not documented and not formatted, but disappears from text if not escaped
                    .Replace("`", "\\`")
                    ;
            }

            return input;
        });

        Expression.Functions.Add("_jsonPrettify", args =>
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

                var data = args[0] is JsonNode n ? JsonConvert.DeserializeObject(n.ToJsonString()) : args[0];

                serializer.Serialize(jtw, data);
            }

            sw.Flush();
            return sw.ToString();
        });

        Expression.Functions.Add("_colorUri", args =>
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
                    var image = new byte[]
                    {
                        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00,
                        0x01, 0x00, 0x80, 0x00, 0x00, (byte)red, (byte)green, (byte)blue,
                        0x00, 0x00, 0x00, 0x21, 0xf9, 0x04, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x2c, 0x00, 0x00, 0x00, 0x00,
                        0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44,
                        0x01, 0x00, 0x3b
                    };

                    // supported formats by AdaptiveCard: PNG, JPEG and GIF
                    // GIF is easiest to generate and smallest one
                    uri = $"data:image/gif;base64,{Convert.ToBase64String(image)}";
                    _colors[key] = uri;
                }

                return uri;
            }

            return string.Empty;
        });
    }
}
