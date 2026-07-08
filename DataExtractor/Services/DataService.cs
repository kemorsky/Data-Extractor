namespace DataExtractor.Services;

using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using System;
using System.Linq;
using System.Collections.Generic;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using DataExtractor.Dto;

public class DataService : IDataService
{
    public async Task<List<LocationDataSheet>> GetLocations(IEnumerable<ILocationGetter> locations, ILinkCache linkCache)
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();

        credential = credential.CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

        var sheetsService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "MyApp"
        });

        var spreadsheetId = "1xugAEYfbjQQbUO5ZWffHO2Q7XaJxG7xRaHVQF1yrvpY";
        var spreadsheetRequest = sheetsService.Spreadsheets.Get(spreadsheetId);

        spreadsheetRequest.Ranges = new List<string>
            {
                "Dungeons!J1:J"
            };
        spreadsheetRequest.IncludeGridData = true;

        var questResponse = await spreadsheetRequest.ExecuteAsync();

        var questCells = questResponse.Sheets[0]
            .Data[0]
            .RowData;

        var request = sheetsService.Spreadsheets.Values.BatchGet(
            spreadsheetId);

        request.Ranges = new List<string>
        {
            "Dungeons!A1:M",
            "Dungeons!M1:M",
        };

        var response = await request.ExecuteAsync();
        var dungeonData = response.ValueRanges;

        Console.WriteLine($"Returned ranges: {dungeonData.Count}");

        foreach (var range in dungeonData)
        {
            Console.WriteLine($"Range: {range.Range}");
            Console.WriteLine($"Rows: {range.Values?.Count ?? 0}");
        }

        var mainData = dungeonData.ElementAtOrDefault(0)?.Values ?? [];

        var sheetLookup = mainData
            .Select((row, i) => new
            {
                Name = row.Count > 0 ? row[0].ToString() : "",
                Row = row,
                QuestCell = questCells.ElementAtOrDefault(i)
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .ToDictionary(
                x => x.Name!,
                StringComparer.OrdinalIgnoreCase);

        var locationsData = new List<LocationDataSheet>();
        var id = 0;

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

            sheetLookup.TryGetValue(
                loc.Name?.ToString() ?? "",
                out var sheet);

            locationsData.Add(new LocationDataSheet
            {
                Id = id++,
                // FormKey = loc.FormKey.ToString(),
                EditorID = loc.EditorID ?? string.Empty,
                ParentLocation = parentNameString,
                Name = displayName ?? string.Empty,
                // Keywords = keywordsList

                Type = sheet?.Row.Count > 3 ? sheet.Row[3].ToString() : null,
                Status = sheet?.Row.Count > 12 ? sheet.Row[12].ToString() : null,

                RelatedQuestName = sheet?.QuestCell?
                    .Values?
                    .FirstOrDefault()?
                    .FormattedValue,

                RelatedQuestUrl = sheet?.QuestCell?
                    .Values?
                    .FirstOrDefault()?
                    .Hyperlink,
            });
        };

        return locationsData;
    }
};