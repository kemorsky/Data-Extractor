namespace DataExtractor.Tool.Services;

using DataExtractor.Tool.Dto;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

public interface IDataService
{
    Task<List<LocationDataSheet>> GetLocations(IEnumerable<ILocationGetter> locations, ILinkCache linkCache);
};