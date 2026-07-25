namespace DataExtractor.Tool.Dto;

using System.Text.Json.Serialization;

public class VikunjaResponseContainer
{
    [JsonPropertyName("items")]
    public List<VikunjaTask> Items { get; set; } = new(); // The inner actual location records
}

public class VikunjaLabel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

public class VikunjaTask
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Name { get; set; } = string.Empty;

    public List<VikunjaLabel> Labels { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}