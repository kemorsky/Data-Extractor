namespace DataExtractor.Tool.Dto;

public class CellData
{
    public int Id { get; set; }
    public string EditorID { get; set; } = string.Empty;
    public string FormKey { get; set; } = string.Empty;
    public int? GridX { get; set; }
    public int? GridY { get; set; }
}