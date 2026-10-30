namespace DataExtractor.Dto;

public class BetacommentData
{
    public int Id { get; set; }
    public string Cell { get; set; } = "";
    public string ReferenceFormKey { get; set; } = "";
    public string RawReference { get; set; } = "";
    public string Comment { get; set; } = "";
    public bool Fixed {get; set;} = false;
    public IList<IList<object>> Rows { get; set; } = [];
}