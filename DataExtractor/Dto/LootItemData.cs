namespace DataExtractor.Dto;

public class LootItemData
{
    public string FormKey { get; set; } = "";
    public string EditorID { get; set; } = "";
    public string Name { get; set; } = "";

    public int Count { get; set; }
    public int Value { get; set; }

    public long TotalValue => (long)Count * Value;
}