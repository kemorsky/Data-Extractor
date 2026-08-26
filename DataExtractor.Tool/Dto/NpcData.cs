namespace DataExtractor.Tool.Dto;

public class NpcData
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public IList<IList<object>> Rows { get; set; } = [];
    public IList<Google.Apis.Sheets.v4.Data.RowData> NpcDocCells { get; set; } = [];
}