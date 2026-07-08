namespace DataExtractor.Services;

using DataExtractor.Dto;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

public interface IModService
{
    Task<List<LocationDataSheet>> GetLocationsFromEsm(IEnumerable<ILocationGetter> locations, ILinkCache linkCache);
};