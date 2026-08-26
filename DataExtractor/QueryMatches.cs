namespace DataExtractor.Tools;

public class DataExtractorTools {

    public static bool QueryMatches(object model, string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;

        var properties = model.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(model);
            if (model == null) continue;

            if (value is string strValue)
            {
                if (strValue.Contains(query, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (value is IEnumerable<string> stringList)
            {
                if (stringList.Any(item => item != null && item.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
        }

        return false;
    }
}