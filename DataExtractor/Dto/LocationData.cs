namespace DataExtractor.Dto;

public class LocationData
{
    public string EditorID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ParentLocation { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
}