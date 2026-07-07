namespace DataExtractor.Services;

using DataExtractor.Dto;

public interface ISheetService
{
    Task<List<LocationDataSheet>> GetLocationsFromSheets();
};