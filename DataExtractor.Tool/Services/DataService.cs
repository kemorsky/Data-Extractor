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
using Noggog;

public class DataService : IDataService
{
    public async Task<List<LocationDataSheet>> GetLocations(
        IEnumerable<ILocationGetter?>? locations = null, 
        ILinkCache? linkCache = null,
        IEnumerable<ICellGetter>? cells = null)
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

        var sheetCells = questResponse.Sheets[0];
        var questCells = sheetCells.Data[0].RowData;
        var imageCells = sheetCells.Data[1].RowData;

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
        var vikunjaApiUrlDungeons = Environment.GetEnvironmentVariable("VIKUNJA_API_URL_DUNGEONS");
        var vikunjaToken = Environment.GetEnvironmentVariable("VIKUNJA_TOKEN");
        var vikunjaFrontendUrl = Environment.GetEnvironmentVariable("VIKUNJA_FRONTEND_URL");
        var vikunjaProjectUrl = Environment.GetEnvironmentVariable("VIKUNJA_PROJECT_URL");

        var vikunjaLookup = new Dictionary<string, (VikunjaTask Task, string Url)>(StringComparer.OrdinalIgnoreCase);

        // DIAGNOSTIC LOG: Check Environment Variables
        Console.WriteLine($"[Vikunja Diagnostics] URL configured: {!string.IsNullOrEmpty(vikunjaApiUrl)} ('{vikunjaApiUrl}')");
        Console.WriteLine($"[Vikunja Diagnostics] Token configured: {!string.IsNullOrEmpty(vikunjaToken)}");
        
        if (!string.IsNullOrEmpty(vikunjaApiUrl) && !string.IsNullOrEmpty(vikunjaToken))
        {
            var apiUrls = new[] { vikunjaApiUrl, vikunjaApiUrlDungeons }
                .Where(url => !string.IsNullOrEmpty(url))
                .ToList();
            
            using var localHttpClient = new HttpClient();

            foreach (var url in apiUrls)
            {
                try
                {
                    // using var localHttpClient = new HttpClient();

                    var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
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

                        Console.Write(tasks);
                        Console.WriteLine($"[Vikunja Diagnostics] Extracted {tasks.Count} inner tasks from API payload.");

                        foreach (var task in tasks)
                        {
                            string rawName = task.Name ?? "";
                            string trimmedKey = rawName.Trim();

                            if (rawName.Contains('-'))
                            {
                                var parts = rawName.Split('-', 2);
                                if (parts.Length > 1) 
                                {
                                    trimmedKey = parts[1].Trim();
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(trimmedKey))
                            {
                                var taskUrl = $"{vikunjaFrontendUrl?.TrimEnd('/')}/tasks/{task.Id}";
                                vikunjaLookup.TryAdd(trimmedKey, (Task: task, Url: taskUrl));
                            }
                        }
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to fetch data from Vikunja API. {ex.Message}");
                }
            }
        }
        
        var locationsData = new List<LocationDataSheet>();
        var id = 0;

        // string rawName = task.Name ?? "";

        // // Strips everything from the start up to the FIRST '.', '-', or ':' and trailing spaces
        // string trimmedKey = Regex.Replace(rawName, @"^(?:[^.:\-]+[.:\-]\s*)+", "").Trim();

        // If mod path is present use both the esm and the masterlist
        foreach (var loc in locations)
        {
            var displayName = loc?.Name?.ToString() ?? "";
            var keywordsList = new List<string>();

            var cellData = new List<CellData>();

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

                // foreach (var cell in cellData)
                // {
                //     Console.WriteLine(
                //     $"  {cell.EditorID} " +
                //     $"Grid: ({cell.GridX}, {cell.GridY})");
                // }

            }

            sheetLookup.TryGetValue(
                displayName,
                out var sheet);

            vikunjaLookup.TryGetValue(displayName, out var vikunjaMatch);

            string rawDesc = vikunjaMatch.Task?.Description ?? "";
            string notesText = "None";
            string vikunjaLink = vikunjaProjectUrl + vikunjaMatch.Task?.Id.ToString() ?? "";

            if (vikunjaMatch.Task?.Id == null)
            {
                vikunjaLink = "";
            }

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
                FormKey = loc?.FormKey.ToString() ?? "None",
                EditorID = loc?.EditorID?.ToString() ?? "None",
                ParentLocation = parentNameString,
                Region = sheet?.Row.Count > 2 ? sheet.Row[2].ToString() ?? "None" : "None",
                Name = displayName,
                Keywords = keywordsList.ToArray(),
                Cells = cellData.ToArray(),
                
                LocationCategory = locationCategory,
                LocationType = locationType,
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