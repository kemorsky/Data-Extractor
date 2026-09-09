namespace DataExtractor.Tool.Services;

using DataExtractor.Tool.Dto;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

public class LootServices : ILootService
{
    public CellLootData AnalyzeCell(
        ICellGetter cell,
        ILinkCache linkCache)
    {
        var results = new CellLootData();

        long directItemValue = 0;
        long containerValues = 0;

        var placedObjects = cell.Persistent
            .Concat(cell.Temporary)
            .OfType<IPlacedObjectGetter>();

        // Group placed instances by their Base FormKey
        var groupedObjects = placedObjects
            .Where(x => !x.Base.IsNull)
            .GroupBy(x => x.Base.FormKey);

        foreach (var group in groupedObjects)
        {
            // var baseLink = group.First().Base;

            // var baseObject = baseLink.TryResolve(linkCache);
            var baseObject = group.First().Base.TryResolve(linkCache);

            if (baseObject == null)
            {
                Console.WriteLine(
                    $"Could not resolve: {group.Key}");
                continue;
            }

            if (baseObject is IContainerGetter container)
            {
                if (container.Items == null)
                {
                    // Console.WriteLine("CONTAINER ENTRY IS NULL");
                    continue;
                }

                foreach (var entry in container.Items)
                {
                    var containerItem = entry.Item;

                    var itemLink = containerItem.Item;

                    if (itemLink.IsNull)
                        continue;

                    IItemGetter containedObject;

                    try
                    {
                        containedObject = linkCache.Resolve<IItemGetter>(
                            itemLink.FormKey);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Could not resolve container item: {itemLink.FormKey} - {ex.Message}");
                        continue;
                    }

                    var containerValue = containedObject
                        .GetType()
                        .GetProperty("Value")?
                        .GetValue(containedObject);

                    if (containerValue == null)
                        continue;

                    var containerUnitValue = Convert.ToInt32(containerValue);

                    var valueFromThisEntry =
                        (long)group.Count()
                        * containerItem.Count
                        * containerUnitValue;

                    containerValues += valueFromThisEntry;
                }

                continue;
            }

            var value = baseObject
                .GetType()
                .GetProperty("Value")?
                .GetValue(baseObject);

            if (value == null)
                continue;

            // var name = baseObject
            //     .GetType()
            //     .GetProperty("Name")?
            //     .GetValue(baseObject);

            // var editorId = baseObject
            //     .GetType()
            //     .GetProperty("EditorID")?
            //     .GetValue(baseObject);

            var count = group.Count();
            var unitValue = Convert.ToInt32(value);

            // var lootItem = new LootItemData
            // {
            //     FormKey = baseObject.FormKey.ToString(),
            //     EditorID = editorId?.ToString() ?? "",
            //     Name = name?.ToString() ?? "",
            //     Count = count,
            //     Value = Convert.ToInt32(value)
            // };

            // Console.WriteLine($"CELLNAME: {cell.Name} LOOT ITEM: {lootItem.FormKey} {lootItem.Name} {lootItem.Value}");

            // results.Items.Add(lootItem);

            var valueFromThisItem =
            (long)count * unitValue;

            directItemValue += valueFromThisItem;
        }

        results.TotalValue = directItemValue + containerValues;

        Console.WriteLine("========================================");
        Console.WriteLine($"LOOT VALUE: {cell.FormKey}");
        Console.WriteLine($"  Direct Items : {directItemValue}");
        Console.WriteLine($"  Containers   : {containerValues}");
        Console.WriteLine($"  -------------------------");
        Console.WriteLine($"  TOTAL        : {results.TotalValue}");
        Console.WriteLine("========================================");

        // results.TotalValue = results.Items.Sum(x => x.TotalValue);

        return results;
    }
}