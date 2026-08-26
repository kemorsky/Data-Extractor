namespace DataExtractor.Tool.Services;

using System.Linq;
using System.Collections.Generic;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using DataExtractor.Tool.Dto;

public class GoogleServices
{

    public async Task<GoogleCredential> GetGoogleCredential()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        return credential.CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
    }

    public SheetsService CreateSheetsService(GoogleCredential credential)
    {
        return new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "MyApp"
        });
    }

    public async Task<GoogleMasterlistData> LoadMasterListData(
        SheetsService sheetsService,
        string masterlistId)
    {
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

        var dungeonsRequest = sheetsService.Spreadsheets.Values.BatchGet(masterlistId);
        dungeonsRequest.Ranges = new List<string>
        {
            "Dungeons!A1:Z",
        };

        var dungeonsResponse = await dungeonsRequest.ExecuteAsync();

        return new GoogleMasterlistData
        {
            Rows = dungeonsResponse.ValueRanges
            .ElementAtOrDefault(0)?
            .Values ?? [],
            QuestCells = questCells
        };
    }

    public async Task<NpcData> LoadNpcData(
        SheetsService sheetsService,
        string npcDatabaseId)
    {
        var npcSpreadsheetRequest = sheetsService.Spreadsheets.Get(npcDatabaseId);
        npcSpreadsheetRequest.Ranges = new List<string>
            {
                "Full List!A1:A",
            };
        npcSpreadsheetRequest.IncludeGridData = true;

        var npcResponse = await npcSpreadsheetRequest.ExecuteAsync();

        var npcSheetCells = npcResponse.Sheets[0];
        var npcDocCells = npcSheetCells.Data[0].RowData;

        // NPC Database ranges
        var npcRequest = sheetsService.Spreadsheets.Values.BatchGet(npcDatabaseId);
        npcRequest.Ranges = new List<string>
        {
            "Full List!A1:Z",
        };

        var npcsResponse = await npcRequest.ExecuteAsync();

        return new NpcData
        {
            Rows = npcsResponse.ValueRanges
            .ElementAtOrDefault(0)?
            .Values ?? [],
            NpcDocCells = npcDocCells
        };
    }
}