namespace DataExtractor.Dto;

public class LocationDataSheet
{
    public int Id { get; set; }
    public string EditorID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ParentLocation { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public string Inhabitants { get; set; } = string.Empty;
    public string RelatedQuestName { get; set; } = string.Empty;
    public string RelatedQuestUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
}