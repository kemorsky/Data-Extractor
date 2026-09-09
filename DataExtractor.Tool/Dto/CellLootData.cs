namespace DataExtractor.Tool.Dto;

public class CellLootData
{
    public long TotalValue { get; set; }

    // Hidden due to extreme memory usage when attempting to fetch the data json (which can reach 500,000 lines)

    // public List<LootItemData> Items { get; set; } = [];
    
    // public List<LootContainerData> Containers { get; set; } = [];
}