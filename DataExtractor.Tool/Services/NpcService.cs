namespace DataExtractor.Tool.Services;

using DataExtractor.Tool.Dto;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

public class NpcServices : INpcService 
{
    public List<NpcData> GetNpcsAtLocation(
        ILocationGetter location,
        IEnumerable<ICellGetter> cells,
        ILinkCache linkCache)
    {
        var results = new List<NpcData>();

        var locationCells = cells.Where(cell => 
            !cell.Location.IsNull &&
            cell.Location.FormKey == location.FormKey);
        
        foreach (var cell in locationCells)
        {
            var placedNpcs = cell.Persistent
                .Concat(cell.Temporary)
                .OfType<IPlacedNpcGetter>()
                .Where(x => !x.Base.IsNull);

            foreach (var placedNpc in placedNpcs)
            {
                
                var npc = placedNpc.Base.TryResolve(linkCache);

                if (npc == null) continue;

                results.Add(new NpcData
                {
                    Name = npc.Name?.ToString()
                        ?? npc.EditorID
                        ?? "Unknown NPC",

                    NpcEditorId = npc.EditorID ?? "",
                    NpcFormKey = npc.FormKey.ToString(),
                    ReferenceFormKey = placedNpc.FormKey.ToString(),
                    CellFormKey = cell.EditorID ?? ""
                });
            }
        }

        return results
            .DistinctBy(npc => npc.ReferenceFormKey)
            .ToList();
    }
}