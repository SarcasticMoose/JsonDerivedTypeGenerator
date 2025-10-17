using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JsonDerivedTypeGenerator.Tests;

public class VectorClassFormatter
{
    private static readonly Regex TokenRegex = new Regex(@"\{@(\w+)\}", RegexOptions.Compiled);

    public static string ReplaceTokens(string input, IDictionary<string, string> values)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return TokenRegex.Replace(
            input,
            match =>
            {
                var key = match.Groups[1].Value;
                if (values.TryGetValue(key, out var value))
                    return value;
                return match.Value;
            }
        );
    }
}
