namespace DataExtractor.Tool.Services;

using System.Text.RegularExpressions;

public static class LocationHelpers
{
    private static string NormalizeLocationKeyword(string keyword)
    {
        var prefixes = new[]
        {
            "LocType",
            "LocSet",
            "BSKLocType",
            "BSKLocSet",
            "CYRLocType",
            "CYRLocSet"
        };

        foreach (var prefix in prefixes)
        {
            if (keyword.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return keyword[prefix.Length..];
            }
        }

        return keyword;
    }

    private static string SplitPascalCase(string value)
    {
        return Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
    }

    private static string FormatLocationKeyword(string keyword)
    {
        var cleaned = NormalizeLocationKeyword(keyword);

        return SplitPascalCase(cleaned);
    }
}