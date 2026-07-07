namespace DataExtractor.Services;

using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using DataExtractor.Dto;

public class SheetService : ISheetService
{
    public async Task<List<LocationDataSheet>> GetLocationsFromSheets()
    {
        var credential = await GoogleCredential
        .GetApplicationDefaultAsync();

        credential = credential.CreateScoped(
            SheetsService.Scope.SpreadsheetsReadonly);

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

        var locationsData = new List<LocationDataSheet>();

        for (int i = 0; i < mainData.Count; i++)
        {
            var row = mainData[i];
            var questCell = questCells.ElementAtOrDefault(i);

            locationsData.Add(new LocationDataSheet
            {
                Id = i,

                Name = row.Count > 0 ? row[0].ToString() : null,
                ParentLocation = row.Count > 1 ? row[1].ToString() : null,
                Type = row.Count > 3 ? row[3].ToString() : null,
                Status = row.Count > 12 ? row[12].ToString() : null,

                RelatedQuestName = questCell?
                    .Values?
                    .FirstOrDefault()?
                    .FormattedValue,

                RelatedQuestUrl = questCell?
                    .Values?
                    .FirstOrDefault()?
                    .Hyperlink,
            }); 
        }

        return locationsData;
    }
};