namespace DataExtractor.Tool.Dto;

public class NpcData
{
    public IList<IList<object>> Rows { get; set; } = [];
    public IList<Google.Apis.Sheets.v4.Data.RowData> NpcDocCells { get; set; } = [];
}