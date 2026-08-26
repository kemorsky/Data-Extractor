namespace DataExtractor.Tool.Dto;

public class GoogleMasterlistData
{
    public IList<IList<object>> Rows { get; set; } = [];
    public IList<Google.Apis.Sheets.v4.Data.RowData> QuestCells { get; set; } = [];
}
