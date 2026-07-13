namespace DataExtractor.Tool.Services;

using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using System;
using System.Linq;
using System.Collections.Generic;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using DataExtractor.Tool.Dto;

public class DataService : IDataService
{
    public async Task<List<LocationDataSheet>> GetLocations(IEnumerable<ILocationGetter?>? locations = null, ILinkCache? linkCache = null)
    {
        var masterlistId = Environment.GetEnvironmentVariable("MASTERLIST-ID");
        
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
        
        var sheetsService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "MyApp"
        });

        var spreadsheetRequest = sheetsService.Spreadsheets.Get(masterlistId);
        spreadsheetRequest.Ranges = new List<string>
            {
                "Dungeons!J1:J",
                "Dungeons!Z1:Z"
            };
        spreadsheetRequest.IncludeGridData = true;

        var questResponse = await spreadsheetRequest.ExecuteAsync();
        

        foreach (var data in questResponse.Sheets[0].Data)
{
            Console.WriteLine($"{data.StartColumn} -> {data.RowData?.Count}");
        }

        var cells = questResponse.Sheets[0];

        var questCells = cells.Data[0].RowData;
        var imageCells = cells.Data[1].RowData;

        var first = imageCells.FirstOrDefault()?.Values?.FirstOrDefault();

        Console.WriteLine(first?.FormattedValue);
        Console.WriteLine(first?.Hyperlink);
        Console.WriteLine(first?.UserEnteredValue?.StringValue);

        Console.WriteLine(cells.Data.Count);

        for (int i = 0; i < cells.Data.Count; i++)
        {
            Console.WriteLine($"Data[{i}] StartColumn={cells.Data[i].StartColumn}");
        }

        var request = sheetsService.Spreadsheets.Values.BatchGet(masterlistId);
        request.Ranges = new List<string>
        {
            "Dungeons!A1:Z",
            // "Dungeons!M1:M",
            // "Dungeons!Y1:Y",
        };

        var response = await request.ExecuteAsync();
        
        var dungeonData = response.ValueRanges;

        Console.WriteLine($"Returned ranges: {dungeonData.Count}");

        var mainData = dungeonData.ElementAtOrDefault(0)?.Values ?? [];

        Console.WriteLine(mainData.Count);
        Console.WriteLine(questCells.Count);
        Console.WriteLine(imageCells.Count);

        var sheetLookup = mainData
            .Select((row, i) => new
            {
                Name = row.Count > 0 ? row[0].ToString() : "",
                Row = row,
                QuestCell = questCells.ElementAtOrDefault(i),
                ImageCell = imageCells.ElementAtOrDefault(i)
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .ToDictionary(
                x => x.Name!,
                StringComparer.OrdinalIgnoreCase);

        var locationsData = new List<LocationDataSheet>();
        var id = 0;

        // If mod path is missing use only the masterlist
        if (locations == null || linkCache == null)
        {
            foreach (var row in sheetLookup)
            {
                var sheet = row.Value;
                
                locationsData.Add(new LocationDataSheet
                {
                    Id = id++,
                    EditorID = sheet?.Row.Count > 6 ? sheet.Row[6].ToString() ?? "None" : "None",
                    ParentLocation = sheet?.Row.Count > 1 ? sheet.Row[1].ToString() ?? "None" : "None",
                    Name = sheet?.Row.Count > 0 ? sheet.Row[0].ToString() ?? "None" : "None",

                    LocationType = sheet?.Row.Count > 3 ? sheet.Row[3].ToString() ?? "None" : "None",
                    Status = sheet?.Row.Count > 12 ? sheet.Row[12].ToString()  ?? "None" : "None",

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

                    Notes = sheet?.Row.Count > 24 ? sheet.Row[24].ToString()  ?? "None" : "None",

                    Image = sheet?.ImageCell?
                        .Values?
                        .FirstOrDefault()?
                        .Hyperlink
                        ?? "None",
                });
            };

            return locationsData;
        };

        // If mod path is present use both the esm and the masterlist
        foreach (var loc in locations)
        {
            var displayName = loc?.Name?.ToString() ?? "";
            var keywordsList = new List<string>();

            sheetLookup.TryGetValue(
                displayName,
                out var sheet);
            
            string parentNameString = "None";

            // Use the FormLink directly instead of pulling the FormKey property out
            if (loc?.ParentLocation != null && loc.ParentLocation.TryResolve(linkCache, out var parentLoc))
            {
                // Console.WriteLine($"Found the parent location! {parentLoc.Name}");
                // FormLink successfully found the record in the active load order cache
                parentNameString = parentLoc.Name?.ToString() ?? parentLoc.EditorID ?? "Unnamed Parent Location";
            }
            else if ((!loc?.ParentLocation.IsNull) ?? false)
            {
                // The link exists but the master file containing it isn't loaded in the environment
                var missingFormKey = loc?.ParentLocation.FormKey;
                Console.WriteLine($"Could not resolve Parent! FormID: {missingFormKey?.ID:X6}, Master File: {missingFormKey?.ModKey}");
                
                parentNameString = missingFormKey.ToString() ?? "";
            }

            // if (loc.Keywords != null)
            // {
            //     keywordsList = loc.Keywords
            //         .Select(keywordLink => {
            //             if (keywordLink.TryResolve(linkCache, out var keywordRecord))
            //             {
            //                 return keywordRecord.EditorID ?? keywordLink.FormKey.ToString();
            //             }
            //             return keywordLink.FormKey.ToString();
            //         })
            //         .ToList();
            // }

            locationsData.Add(new LocationDataSheet
            {
                Id = id++,
                // FormKey = loc.FormKey.ToString(),
                EditorID = loc?.EditorID?.ToString() ?? "None",
                ParentLocation = parentNameString,
                Name = displayName,
                // Keywords = keywordsList

                LocationType = sheet?.Row.Count > 3 ? sheet.Row[3].ToString() ?? "None" : "None",
                Status = sheet?.Row.Count > 12 ? sheet.Row[12].ToString()  ?? "None" : "None",

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

                Notes = sheet?.Row.Count > 24 ? sheet.Row[24].ToString()  ?? "None" : "None",

                Image = sheet?.ImageCell?
                    .Values?
                    .FirstOrDefault()?
                    .Hyperlink
                    ?? "None",
            });
        };

        Console.WriteLine($"Locations from DataService: {locationsData.Count}");

        return locationsData;
    }
};