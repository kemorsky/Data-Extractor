namespace DataExtractor.Tool.Helpers;

using System.Text.RegularExpressions;

public class LocationTypeAndCategoryNormalizer
{
    public static string KeywordExtractor(string keyword)
    {
        var cleaned = Regex.Replace(
            keyword,
            @"^(LocType|LocSet)",
            "",
            RegexOptions.IgnoreCase
        );

        return Regex.Replace(
            cleaned,
            @"(?<!^)([A-Z])",
            " $1"
        ).Trim();
    }
}