namespace DataExtractor.Tool.Services;

using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using DataExtractor.Tool.Dto;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;
using Noggog;

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

        var cells = questResponse.Sheets[0];
        var questCells = cells.Data[0].RowData;
        var imageCells = cells.Data[1].RowData;

        var request = sheetsService.Spreadsheets.Values.BatchGet(masterlistId);
        request.Ranges = new List<string>
        {
            "Dungeons!A1:Z",
        };

        var response = await request.ExecuteAsync();

        var dungeonData = response.ValueRanges;
        Console.WriteLine($"Returned ranges: {dungeonData.Count}");

        var mainData = dungeonData.ElementAtOrDefault(0)?.Values ?? [];

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

        // Vikunja Environment Variables
        var vikunjaApiUrl = Environment.GetEnvironmentVariable("VIKUNJA_API_URL");
        var vikunjaToken = Environment.GetEnvironmentVariable("VIKUNJA_TOKEN");
        var vikunjaFrontendUrl = Environment.GetEnvironmentVariable("VIKUNJA_FRONTEND_URL");

        var vikunjaLookup = new Dictionary<string, (VikunjaTask Task, string Url)>(StringComparer.OrdinalIgnoreCase);

        // DIAGNOSTIC LOG: Check Environment Variables
        Console.WriteLine($"[Vikunja Diagnostics] URL configured: {!string.IsNullOrEmpty(vikunjaApiUrl)} ('{vikunjaApiUrl}')");
        Console.WriteLine($"[Vikunja Diagnostics] Token configured: {!string.IsNullOrEmpty(vikunjaToken)}");
        
        if (!string.IsNullOrEmpty(vikunjaApiUrl) && !string.IsNullOrEmpty(vikunjaToken))
        {
            try
            {
                using var localHttpClient = new HttpClient();

                var httpRequest = new HttpRequestMessage(HttpMethod.Get, vikunjaApiUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", vikunjaToken);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var httpResponse = await localHttpClient.SendAsync(httpRequest);

                Console.WriteLine($"[Vikunja Diagnostics] HTTP Status Code Received: {httpResponse.StatusCode} ({(int)httpResponse.StatusCode})");

                if (httpResponse.IsSuccessStatusCode)
                {
                    var jsonString = await httpResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var responseData = JsonSerializer.Deserialize<VikunjaResponseContainer>(jsonString, options);

                    var tasks = responseData?.Items ?? new List<VikunjaTask>();

                    Console.WriteLine($"[Vikunja Diagnostics] Extracted {tasks.Count} inner tasks from API payload.");

                    vikunjaLookup = tasks
                        .Select(x => 
                        {
                            string rawName = x.Name;
                            string trimmedKey = rawName.Trim();

                            // Check and trim the Vikunja task title string
                            if (rawName.Contains('-'))
                            {
                                var parts = rawName.Split('-', 2);
                                if (parts.Length > 1) 
                                {
                                    trimmedKey = parts[1].Trim(); // E.g., Returns "Sarvena's House"
                                }
                            }
                            return new { TrimmedKey = trimmedKey, Task = x };
                        })
                        .DistinctBy(x => x.TrimmedKey, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary( 
                            x => x.TrimmedKey,
                            x => (t: x.Task, Url: $"{vikunjaFrontendUrl?.TrimEnd('/')}/tasks/{x.Task.Id}"),
                            StringComparer.OrdinalIgnoreCase
                        );

                    Console.WriteLine($"Successfully loaded {vikunjaLookup.Count} trimmed keys from Vikunja.");
                    foreach (var key in vikunjaLookup.Keys.Take(5)) 
                    {
                        Console.WriteLine($"Parsed Vikunja Key Preview: '{key}'");
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to fetch data from Vikunja API. {ex.Message}");
            }
        }
        
        var locationsData = new List<LocationDataSheet>();
        var id = 0;

        // If mod path is missing use only the masterlist
        // if (locations == null || linkCache == null)
        // {
        //     foreach (var row in sheetLookup)
        //     {
        //         var sheet = row.Value;

        //         var locationName = sheet?.Row.Count > 0 ? sheet.Row[0].ToString() ?? "None" : "None";

        //         vikunjaLookup.TryGetValue(locationName, out var vikunjaMatch);

        //         string rawDesc = vikunjaMatch.Task?.Description ?? "";
        //         string notesText = "None";

        //         if (!string.IsNullOrWhiteSpace(rawDesc))
        //         {
        //             // Split on <ul followed by any characters until the closing > bracket <ul>, <ul class="...">, <ul style="...">, etc.
        //             var parts = Regex.Split(rawDesc, @"<h2\b[^>]*>\s*Checklist|<ul\b[^>]*>|<ol\b[^>]*>", RegexOptions.IgnoreCase);
                    
        //             // Keep everything before the first ul container
        //             notesText = parts[0].Trim();
        //         }
                
        //         locationsData.Add(new LocationDataSheet
        //         {
        //             Id = id++,
        //             EditorID = sheet?.Row.Count > 6 ? sheet.Row[6].ToString() ?? "None" : "None",
        //             ParentLocation = sheet?.Row.Count > 1 ? sheet.Row[1].ToString() ?? "None" : "None",
        //             Region = sheet?.Row.Count > 2 ? sheet.Row[2].ToString() ?? "None" : "None",
        //             Name = locationName,

        //             LocationType = sheet?.Row.Count > 3 ? sheet.Row[3].ToString() ?? "None" : "None",
        //             Inhabitants = sheet?.Row.Count > 5 ? sheet.Row[5].ToString() ?? "None" : "None",
        //             Status = sheet?.Row.Count > 12 ? sheet.Row[12].ToString()  ?? "None" : "None",

        //             RelatedQuestName = sheet?.QuestCell?
        //                 .Values?
        //                 .FirstOrDefault()?
        //                 .FormattedValue
        //                 ?? "None",

        //             RelatedQuestUrl = sheet?.QuestCell?
        //                 .Values?
        //                 .FirstOrDefault()?
        //                 .Hyperlink
        //                 ?? "None",

        //             HasQuest = (sheet?.QuestCell?
        //                 .Values?
        //                 .FirstOrDefault()?
        //                 .FormattedValue
        //                 ?? "None") != "None",

        //             Notes = !string.IsNullOrWhiteSpace(notesText) ? notesText : "None",

        //             // Notes = vikunjaMatch.Task != null && !string.IsNullOrWhiteSpace(vikunjaMatch.Task.Description) 
        //             //     ? vikunjaMatch.Task.Description 
        //             //     : (sheet?.Row.Count > 24 ? sheet.Row[24].ToString()  ?? "None" : "None"),

        //             Image = sheet?.ImageCell?
        //                 .Values?
        //                 .FirstOrDefault()?
        //                 .Hyperlink
        //                 ?? "None",
        //         });
        //     };

        //     return locationsData;
        // };

        // If mod path is present use both the esm and the masterlist
        foreach (var loc in locations)
        {
            var displayName = loc?.Name?.ToString() ?? "";
            var keywordsList = new List<string>();

            sheetLookup.TryGetValue(
                displayName,
                out var sheet);

            vikunjaLookup.TryGetValue(displayName, out var vikunjaMatch);

            string rawDesc = vikunjaMatch.Task?.Description ?? "";
            string notesText = "None";

            if (!string.IsNullOrWhiteSpace(rawDesc))
            {
                // Split on <ul followed by any characters until the closing > bracket <ul>, <ul class="...">, <ul style="...">, etc.
                var parts = Regex.Split(rawDesc, @"<h2\b[^>]*>\s*Checklist|<ul\b[^>]*>|<ol\b[^>]*>", RegexOptions.IgnoreCase);
                
                // Keep everything before the first ul container
                notesText = parts[0].Trim();
            }

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
                // Console.WriteLine($"Found the parent location! {parentLoc}");
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
                // FormKey = loc?.FormKey.ToString(),
                EditorID = loc?.EditorID?.ToString() ?? "None",
                ParentLocation = parentNameString,
                Region = sheet?.Row.Count > 2 ? sheet.Row[2].ToString() ?? "None" : "None",
                Name = displayName,
                Keywords = keywordsList.ToArray(),
                
                LocationCategory = locationCategory,
                LocationType = locationType,
                Inhabitants = sheet?.Row.Count > 5 ? sheet.Row[5].ToString() ?? "None" : "None",
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

                Notes = !string.IsNullOrWhiteSpace(notesText) ? notesText : "None",

                // Notes = vikunjaMatch.Task != null && !string.IsNullOrWhiteSpace(vikunjaMatch.Task.Description) 
                //     ? vikunjaMatch.Task.Description 
                //     : (sheet?.Row.Count > 24 ? sheet.Row[24].ToString()  ?? "None" : "None"),

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