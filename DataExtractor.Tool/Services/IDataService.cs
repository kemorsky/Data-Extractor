namespace DataExtractor.Tool.Services;

using DataExtractor.Tool.Dto;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Google.Apis.Auth.OAuth2;
using System.Collections.Generic;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

public interface IDataService
{
    Task<List<LocationDataSheet>> GetLocations(
        IEnumerable<ILocationGetter> locations, 
        ILinkCache linkCache, 
        IEnumerable<ICellGetter> cells
    );

    
};