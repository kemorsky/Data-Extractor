namespace DataExtractor.Tool.Dto;

public class NpcData
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public string NpcEditorId { get; set; } = "";
    public string NpcFormKey { get; set; } = "";
    public string ReferenceFormKey { get; set; } = "";
    public string CellFormKey { get; set; } = "";
    public IList<IList<object>> Rows { get; set; } = [];
    public IList<Google.Apis.Sheets.v4.Data.RowData> NpcDocCells { get; set; } = [];
}