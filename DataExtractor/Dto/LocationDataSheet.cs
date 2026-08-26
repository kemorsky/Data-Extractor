namespace DataExtractor.Dto;

public class LocationDataSheet
{
    public int Id { get; set; }
    public string EditorID { get; set; } = string.Empty;
    public string FormKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ParentLocation { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = [];
    public CellData[] Cells { get; set; } = Array.Empty<CellData>();
    public string LocationCategory { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public NpcData[] InhabitingNpcs { get; set; } = [];
    public string[] Inhabitants { get; set; } = [];
    public string RelatedQuestName { get; set; } = string.Empty;
    public string RelatedQuestUrl { get; set; } = string.Empty;
    public bool HasQuest { get; set; } = false;
    public string Status { get; set; } = string.Empty;
    public string VikunjaLink { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}