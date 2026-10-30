namespace DataExtractor.Tool.Services;

using Mutagen.Bethesda;
using Mutagen.Bethesda.Analyzers;
using Mutagen.Bethesda.Analyzers.Skyrim;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DataExtractor.Tool.Dto;
using DataExtractor.Tool.Helpers;
using Noggog;
using System.Reflection;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;

public class DataService : IDataService
{
    private readonly GoogleServices _googleServices;
    private readonly VikunjaServices _vikunjaServices;
    private readonly ILootService _lootServices;
    private readonly INpcService _npcServices;

    public DataService(GoogleServices googleServices, VikunjaServices vikunjaServices, LootServices lootServices, NpcServices npcServices)
    {
        _googleServices = googleServices;
        _vikunjaServices = vikunjaServices;
        _lootServices = lootServices;
        _npcServices = npcServices;
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
        var betacommentDatabaseId = Environment.GetEnvironmentVariable("BETACOMMENTS_DATABASE_ID")
            ?? throw new InvalidOperationException("BETACOMMENTS_DATABASE_ID environment variable is not set.");
        
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

        var betacommentData = await _googleServices.GetBetacomments(
            sheetsService,
            betacommentDatabaseId
        );

        // Dungeon Masterlist data
        var mainData = masterlistData.Rows;
        var questCells = masterlistData.QuestCells;

        // Npc Database data
        var mainNpcData = npcData.Rows;
        var npcDocCells = npcData.NpcDocCells;

        // Betacomment Database data
        var mainBetacommentData = betacommentData.Rows;

        Console.WriteLine($"[NPC DEBUG] Total NPC rows returned: {mainNpcData.Count}");

        Console.WriteLine($"[NPC DEBUG] Total Betacomment rows returned: {mainBetacommentData.Count}");

        var sheetLookup = mainData
            .Select((row, i) => new
            {
                Name = row.Count > 0 ? row[0].ToString()?.Trim() : "",
                Row = row,
                QuestCell = questCells.ElementAtOrDefault(i),
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new
            {
                x.Name,
                NormalizedName = LocationNameNormalizer.Normalize(x.Name!),
                x.Row,
                x.QuestCell
            })
            .GroupBy(x => x.NormalizedName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.First(),
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
                g => g.ToList(),
                StringComparer.OrdinalIgnoreCase);

        Console.WriteLine(
            $"[NPC LOOKUP] Created {npcLookup.Count} location entries.");

        var betacommentLookup = mainBetacommentData
            .Select((row, i) => new BetacommentData
            {
                Cell = row.Count > 9 ? row[9].ToString()?.Trim() ?? "" : "",
                ReferenceFormKey = row.Count > 10 ? row[10].ToString()?.Trim() ?? "" : "",
                RawReference = row.Count > 12 ? row[12].ToString()?.Trim() ?? "" : "",
                Comment = row.Count > 11 ? row[11].ToString()?.Trim() ?? "" : "",
                Fixed = row.Count > 0 && bool.TryParse(row[0]?.ToString(), out var fixedValue) &&
                    fixedValue
            })
            // .Select((x) => new
            // {
            //     x.Cell,
            //     x.ReferenceFormKey,
            //     x.Comment,
            //     x.RawReferenceWithComment,
            //     LocationName = LocationNameNormalizer.Normalize(x.Cell),
            //     x.Row,
            // })
            .Where(x => 
                !string.IsNullOrWhiteSpace(x.Cell))
            .GroupBy(x => LocationNameNormalizer.Normalize(x.Cell), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.ToList(),
                StringComparer.OrdinalIgnoreCase);

        // Console.WriteLine(
        //     $"[BETA DEBUG] Lookup keys: {betacommentLookup.Count}");

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
            var betacommentsList = new List<BetacommentData>();

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
                    var loot = _lootServices.AnalyzeCell(cell, linkCache);
                    
                    // Console.WriteLine(
                    //     $"[LOOT CHECK] Cell={cell.FormKey} " +
                    //     $"TotalValue={loot.TotalValue}");

                    cellData.Add(new CellData
                    {
                        Id = id++,
                        EditorID = cell.EditorID ?? "",
                        FormKey = cell.FormKey.ToString(),
                        GridX = cell.Grid?.Point.X,
                        GridY = cell.Grid?.Point.Y,
                        Loot = loot
                    });

                betacommentsList = cellsForLocation
                    .Where(cell => !string.IsNullOrWhiteSpace(cell.EditorID))
                    .SelectMany(cell =>
                    {
                        var cellKey = LocationNameNormalizer.Normalize(cell.EditorID);

                        return betacommentLookup.TryGetValue(cellKey, out var matches)
                            ? matches
                            : Enumerable.Empty<BetacommentData>();
                    })
                    .Select(comment => new BetacommentData
                    {
                        Id = ++id,
                        Cell = comment.Cell,
                        ReferenceFormKey = comment.ReferenceFormKey,
                        Comment = comment.Comment,
                        RawReference = comment.RawReference,
                        Fixed = comment.Fixed
                    })
                    .ToList();
                }
            }

            // Npc placement data pulled from esm
            var npcResults = _npcServices.GetNpcsAtLocation(
                loc,
                cells,
                linkCache);

            var groupedNpcResults = npcResults
                .GroupBy(
                    npc => LocationNameNormalizer.Normalize(npc.Name),
                    StringComparer.OrdinalIgnoreCase)
                .Select(group => new
                {
                    Name = group.First().Name ?? "Unknown NPC",
                    Count = group.Count(),
                    NpcFormKey = group.First().NpcFormKey
                })
                .ToList();

            // foreach (var npc in npcResults)
            // {
            //     Console.WriteLine(
            //         $"NPC: {npc.Name} | " +
            //         $"EditorID: {npc.NpcEditorId} | " +
            //         $"FormKey: {npc.NpcFormKey} | " +
            //         $"Reference: {npc.ReferenceFormKey} | " +
            //         $"Cell: {npc.CellFormKey}");
            // }

            sheetLookup.TryGetValue(
                locationKey,
                out var sheet);

            npcLookup.TryGetValue(
                locationKey,
                out var npcs);

            // betacommentLookup.TryGetValue(
            //     locationKey,
            //     out var betacomments);

            // Console.WriteLine(
            //     $"[BETACOMMENT MATCH] Location='{displayName}', " +
            //     $"Key='{locationKey}', " +
            //     $"Found={foundBetacomments}, " +
            //     $"Count={betacomments?.Count ?? 0}");

            npcs ??= [];

            vikunjaLookup.TryGetValue(locationKey, out var vikunjaMatch);

            string rawDesc = vikunjaMatch.Task?.Description ?? "";
            string notesText = "None";

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
                Cells = cellData
                    .OrderBy(c => c.EditorID)
                    .ToArray(),
                
                Betacomments = betacommentsList
                    .OrderBy(c => c.Cell)
                    .ToArray(),
                
                LocationCategory = locationCategory,
                LocationType = locationType,

                InhabitingNpcs = groupedNpcResults
                    .Select(npc => new NpcData
                    {
                        Name = npc.Count > 1 // If multiple copies of an NPC in a cell group them together
                            ? $"{npc.Name} ({npc.Count})"
                            : npc.Name,
                        Url = npcs
                            .FirstOrDefault(sheetNpc =>
                                string.Equals(
                                    LocationNameNormalizer.Normalize(sheetNpc.Name),
                                    LocationNameNormalizer.Normalize(npc.Name),
                                    StringComparison.OrdinalIgnoreCase))
                            ?.NpcDocCell?
                            .Values?
                            .FirstOrDefault()?
                            .Hyperlink ?? ""
                    })
                    .OrderBy(n => n.Name)
                    .ToArray(),

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

                VikunjaLink = vikunjaMatch.Url ?? "",

                Notes = !string.IsNullOrWhiteSpace(notesText) ? notesText : "None",
            });
        };

        Console.WriteLine($"Locations from DataService: {locationsData.Count}");

        return locationsData;
    }
};