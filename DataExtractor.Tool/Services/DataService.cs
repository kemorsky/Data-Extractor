namespace DataExtractor.Tool.Services;

using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DataExtractor.Tool.Dto;
using DataExtractor.Tool.Helpers;
using Noggog;

public class DataService : IDataService
{
    private readonly GoogleServices _googleServices;
    private readonly VikunjaServices _vikunjaServices;

    public DataService(GoogleServices googleServices, VikunjaServices vikunjaServices)
    {
        _googleServices = googleServices;
        _vikunjaServices = vikunjaServices;
    }

    public async Task<List<LocationDataSheet>> GetLocations(
        IEnumerable<ILocationGetter?> locations, 
        ILinkCache linkCache,
        IEnumerable<ICellGetter> cells)
    {
        // Google envs
        var masterlistId = Environment.GetEnvironmentVariable("MASTERLIST_ID") 
            ?? throw new InvalidOperationException("MASTERLIST_ID environment variable is not set.");
        var npcDatabaseId = Environment.GetEnvironmentVariable("NPC_DATABASE_ID")
            ?? throw new InvalidOperationException("NPC_DATABASE_ID environment variable is not set.");
        
        var credential = await _googleServices.GetGoogleCredential();
        
        var sheetsService = _googleServices.CreateSheetsService(credential);
        var npcDatabaseService = _googleServices.CreateSheetsService(credential);

        var masterlistData = await _googleServices.LoadMasterListData(
            sheetsService,
            masterlistId
        );

         var npcData = await _googleServices.LoadNpcData(
            sheetsService,
            npcDatabaseId
        );

        // Dungeon Masterlist data
        var mainData = masterlistData.Rows;
        var questCells = masterlistData.QuestCells;

        // Npc Database data
        var mainNpcData = npcData.Rows;
        var npcDocCells = npcData.NpcDocCells;

        Console.WriteLine($"[NPC DEBUG] Total NPC rows returned: {mainNpcData.Count}");

        var sheetLookup = mainData
            .Select((row, i) => new
            {
                Name = row.Count > 0 ? row[0].ToString()?.Trim() : "",
                Row = row,
                QuestCell = questCells.ElementAtOrDefault(i),
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .ToDictionary(
                x => x.Name!,
                StringComparer.OrdinalIgnoreCase);

        var npcLookup = mainNpcData
            .Select((row, i) => new
            {
                Name = row.Count > 0 ? row[0].ToString()?.Trim() : "",
                RawLocationName = row.Count > 7
                    ? row[7]?.ToString()?.Trim()
                    : "",
                Row = row,
                NpcDocCell = npcDocCells.ElementAtOrDefault(i),
            })
            .Select((x) => new
            {
                x.Name,
                LocationName = LocationNameNormalizer.Normalize(x.RawLocationName),
                x.Row,
                x.NpcDocCell
            })
            .Where(x => 
                !string.IsNullOrWhiteSpace(x.Name) && 
                !string.IsNullOrWhiteSpace(x.LocationName))
            .GroupBy(x => x.LocationName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.First(),
                StringComparer.OrdinalIgnoreCase);

        Console.WriteLine(
            $"[NPC LOOKUP] Created {npcLookup.Count} location entries.");

        foreach (var entry in npcLookup.Take(30))
        {
            Console.WriteLine(
                $"[NPC LOOKUP] " +
                $"Key='{entry.Key}' -> " +
                $"NPC='{entry.Value.Name}'");
        }

        var vikunjaProjectUrl = Environment.GetEnvironmentVariable("VIKUNJA_PROJECT_URL");

        var vikunjaLookup = await _vikunjaServices.LoadVikunjaData();
        
        var locationsData = new List<LocationDataSheet>();
        var id = 0;

        // If mod path is present use both the esm and the masterlist
        foreach (var loc in locations)
        {
            var displayName = loc?.Name?.ToString() ?? "";
            var locationKey = LocationNameNormalizer.Normalize(displayName);

            var keywordsList = new List<string>();

            var cellData = new List<CellData>();

            // Grid coordinates pulled from esm
            if (loc != null && cells != null)
            {
                var cellsForLocation = System.Linq.Enumerable
                    .Where(
                        cells,
                        cell =>
                            !cell.Location.IsNull &&
                            cell.Location.FormKey == loc.FormKey)
                    .ToList();

                foreach (var cell in cellsForLocation)
                {
                    cellData.Add(new CellData
                    {
                        Id = id++,
                        EditorID = cell.EditorID ?? "",
                        FormKey = cell.FormKey.ToString(),
                        GridX = cell.Grid?.Point.X,
                        GridY = cell.Grid?.Point.Y
                    });
                }
            }

            sheetLookup.TryGetValue(
                locationKey,
                out var sheet);

            npcLookup.TryGetValue(
                locationKey,
                out var npc);

            vikunjaLookup.TryGetValue(locationKey, out var vikunjaMatch);

            string rawDesc = vikunjaMatch.Task?.Description ?? "";
            string notesText = "None";
            string vikunjaLink = vikunjaProjectUrl + vikunjaMatch.Task?.Id.ToString() ?? "";

            if (vikunjaMatch.Task?.Id == null)
            {
                vikunjaLink = "";
            }

            // Vikunja card description clip (removes list containers)
            if (!string.IsNullOrWhiteSpace(rawDesc))
            {
                // Split on <ul followed by any characters until the closing > bracket <ul>, <ul class="...">, <ul style="...">, etc.
                var parts = Regex.Split(rawDesc, @"<h2\b[^>]*>\s*Checklist|<ul\b[^>]*>|<ol\b[^>]*>", RegexOptions.IgnoreCase);
                
                // Keep everything before the first ul container
                notesText = parts[0].Trim();
            }

            // Vikunja label handling and matching
            string displayStatusText = "None";
            if (vikunjaMatch.Task?.Labels != null)
            {
                var matchedLabel = vikunjaMatch.Task.Labels
                    .FirstOrDefault(l => DataExtractorConfig.AllowedLabelIds.Contains(l.Id));

                if (matchedLabel != null)
                {
                    string rawTitle = matchedLabel.Title.Trim();

                    displayStatusText = DataExtractorConfig.StatusTranslationMap.TryGetValue(rawTitle, out var unionizedStatus)
                        ? unionizedStatus
                        : rawTitle;
                }
            };

            string parentNameString = "None";

            // Use the FormLink directly instead of pulling the FormKey property out
            if (loc?.ParentLocation != null && loc.ParentLocation.TryResolve(linkCache, out var parentLoc))
            {
                // FormLink successfully found the record in the active load order cache
                parentNameString = parentLoc.Name?.ToString() ?? parentLoc.EditorID ?? "Unnamed Parent Location";
            }
            else if ((!loc?.ParentLocation.IsNull) ?? false)
            {
                // The link exists but the master file containing it isn't loaded in the environment
                var missingFormKey = loc?.ParentLocation.FormKey;
                Console.WriteLine($"Could not resolve Parent! FormID: {missingFormKey?.ID:X6}, Master File: {missingFormKey?.ModKey}");
                
                parentNameString = missingFormKey.ToString() ?? "";
            };

            if (loc?.Keywords != null)
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
            };

            string locationCategory = "None";
            string locationType = "None";

            foreach (var keyword in keywordsList)
            {
                if (DataExtractorConfig.LocationCategoryMap.TryGetValue(keyword, out var category))
                {
                    locationCategory = category;
                }
                if (DataExtractorConfig.LocationTypeMap.TryGetValue(keyword, out var type))
                {
                    locationType = type;
                }
            }

            locationsData.Add(new LocationDataSheet
            {
                Id = id++,
                FormKey = loc?.FormKey.ToString() ?? "None",
                EditorID = loc?.EditorID?.ToString() ?? "None",
                ParentLocation = parentNameString,
                Region = sheet?.Row.Count > 2 ? sheet.Row[2].ToString() ?? "None" : "None",
                Name = displayName,
                Keywords = keywordsList.ToArray(),
                Cells = cellData.ToArray(),
                
                LocationCategory = locationCategory,
                LocationType = locationType,

                InhabitingNpcName = npc?.Name ?? "None",

                InhabitingNpcUrl = npc?.NpcDocCell?
                    .Values?
                    .FirstOrDefault()?
                    .Hyperlink
                    ?? "None",

                Inhabitants = sheet?.Row.Count > 5 && sheet.Row[5] != null
                    ? sheet.Row[5].ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    : Array.Empty<string>(),

                Status = displayStatusText != "None" 
                    ? displayStatusText 
                    : (sheet?.Row.Count > 12 ? sheet.Row[12].ToString()  ?? "None" : "None"),

                RelatedQuestName = sheet?.QuestCell?
                    .Values?
                    .FirstOrDefault()?
                    .FormattedValue
                    ?? "None",

                RelatedQuestUrl = sheet?.QuestCell?
                    .Values?
                    .FirstOrDefault()?
                    .Hyperlink
                    ?? "None",

                HasQuest = (sheet?.QuestCell?
                    .Values?
                    .FirstOrDefault()?
                    .FormattedValue
                    ?? "None") != "None",

                VikunjaLink = vikunjaLink,

                Notes = !string.IsNullOrWhiteSpace(notesText) ? notesText : "None",
            });
        };

        Console.WriteLine($"Locations from DataService: {locationsData.Count}");

        return locationsData;
    }
};