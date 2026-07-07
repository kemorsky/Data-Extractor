namespace DataExtractor.Dto;

public class LocationDataSheet
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ParentLocation { get; set; }
    public string? Type { get; set; }
    public string? RelatedQuestName { get; set; }
    public string? RelatedQuestUrl { get; set; }
    public string? Status { get; set; }
}