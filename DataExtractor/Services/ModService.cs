namespace DataExtractor.Services;

using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System;
using System.Linq;
using System.Collections.Generic;
using Mutagen.Bethesda.Plugins.Cache;
using DataExtractor.Dto;

public class ModService : IModService
{
    public Task<List<LocationDataSheet>> GetLocationsFromEsm(IEnumerable<ILocationGetter> locations, ILinkCache linkCache)
    {
        var namesList = new List<LocationDataSheet>();

        foreach (var loc in locations)
        {
            var displayName = loc.Name?.ToString();
            var keywordsList = new List<string>();
            
            string parentNameString = "None";

            // Use the FormLink directly instead of pulling the FormKey property out
            if (loc.ParentLocation.TryResolve(linkCache, out var parentLoc))
            {
                Console.WriteLine($"Found the parent location! {parentLoc.Name}");
                // FormLink successfully found the record in the active load order cache
                parentNameString = parentLoc.Name?.ToString() ?? parentLoc.EditorID ?? "Unnamed Parent Location";
            }
            else if (!loc.ParentLocation.IsNull)
            {
                // The link exists but the master file containing it isn't loaded in the environment
                var missingFormKey = loc.ParentLocation.FormKey;
                Console.WriteLine($"Could not resolve Parent! FormID: {missingFormKey.ID:X6}, Master File: {missingFormKey.ModKey}");
                
                parentNameString = missingFormKey.ToString();
            }

            if (loc.Keywords != null)
            {
                keywordsList = loc.Keywords
                    .Select(keywordLink => {
                        if (keywordLink.TryResolve(linkCache, out var keywordRecord))
                        {
                            return keywordRecord.EditorID ?? keywordLink.FormKey.ToString();
                        }
                        return keywordLink.FormKey.ToString();
                    })
                    .ToList();
            }

            var details = new LocationDataSheet
            {
                // FormKey = loc.FormKey.ToString(),
                EditorID = loc.EditorID ?? string.Empty,
                ParentLocation = parentNameString,
                Name = displayName ?? string.Empty,
                // Keywords = keywordsList
            };

            namesList.Add(details);
        }
        
        return Task.FromResult(namesList.Distinct().ToList());
    }
}