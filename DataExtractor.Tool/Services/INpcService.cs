namespace DataExtractor.Tool.Services;

using DataExtractor.Tool.Dto;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

public interface INpcService
{
    public List<NpcData> GetNpcsAtLocation(
        ILocationGetter location,
        IEnumerable<ICellGetter> cells,
        ILinkCache linkCache);
}