namespace DataExtractor.Tool.Dto;

public class LootContainerData
{
    public string FormKey { get; set; } = "";
    public string EditorID { get; set; } = "";
    public string Name { get; set; } = "";
    public int Count { get; set; }
    public long TotalValue { get; set; }

    public List<LootItemData> Items { get; set; } = [];
}