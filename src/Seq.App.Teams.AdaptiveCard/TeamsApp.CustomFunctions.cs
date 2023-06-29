﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                    var image = new byte[]
                    {
                        0x42, 0x4d, 0x3a, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
                        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00,
                        0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0xc3, 0x0e,
                        0x00, 0x00, 0xc3, 0x0e, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, (byte)blue, (byte)green,
                        (byte)red, 0x00
                    };

                    uri = $"data:image/bmp;base64,{Convert.ToBase64String(image)}";
                    _colors[key] = uri;
                }

                return uri;
            }

            return string.Empty;
        });
    }
}
