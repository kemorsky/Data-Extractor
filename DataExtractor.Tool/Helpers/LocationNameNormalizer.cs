namespace DataExtractor.Tool.Helpers;

using System.Text.RegularExpressions;

public class LocationNameNormalizer{

    public static string Normalize(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            var results = Regex.Replace(
                name.Trim(),
                @"^(?:[^.:]+[.:]\s*|[^-]+-\s*)",
                "",
                RegexOptions.IgnoreCase);

            // Normalize whitespace
            results = Regex.Replace(results, @"\s+", " ");

            // Remove punctuation
            results = Regex.Replace(results, @"[^\p{L}\p{N}\s]", "");

            // Remove spaces
            results = results.Replace(" ", "");

            Console.WriteLine(
                $"[NPC NORMALIZE] " +
                $"Raw='{name}' -> Normalized='{results}'");

            return results.Trim().ToLowerInvariant();

        }
}