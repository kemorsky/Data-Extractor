using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;
using DataExtractor.Services;
using DataExtractor.Dto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<ISheetService, SheetService>();
builder.Services.AddScoped<IModService, ModService>();

// CORS builder services
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.MapOpenApi();

List<LocationDataSheet> locationsCache = new();

var sheetService = new SheetService();
var modService = new ModService();

// TODO - combine both fetches under the same umbrella

// app.MapGet("/locatins", async (IModService modServices, ISheetService sheetServices) =>
// {
//     var sheetTask = new SheetService();
//     var modTask = new ModService();

//     await Task.WhenAll(sheetTask, modTask);

//     var sheetData = await sheetTask.GetLocationsFromSheets();
//     var modData = await modTask.GetLocationsFromEsm();

//     var result = sheetData.Select(sheet =>
//     {
//         var mod = modData.FirstOrDetault(m => m.Name == sheet.Name);

//         return new
//         {
//             Sheet = sheet,
//             Mod = mod
//         }
//     });

//     return Results.Ok(result);
// });

// Google Sheets endpoint
app.MapGet("/test", async () =>
{
    locationsCache = await sheetService.GetLocationsFromSheets();

    return Results.Ok(locationsCache);
    
});

// Google Sheets endpoint
app.MapGet("/test/{id:int}", async (int id) =>
{
    var location = locationsCache
        .FirstOrDefault(x => x.Id == id);

        return location == null 
            ? Results.NotFound()
            : Results.Ok(location);
});

var pathToModHeartlandESM = @"C:\Modding\MO2\mods\se-heartlands";
var pathToModAssetsESM = @"C:\Modding\MO2\mods\se-assets-master";

app.MapGet("/mod-data", () =>
{
    using var env = GameEnvironment.Typical.Skyrim(SkyrimRelease.SkyrimSE);
    ILinkCache linkCache = env.LinkCache;

    // var weapons = ModHelper.GetEditorIds(env.LoadOrder.PriorityOrder.Weapon().WinningOverrides());

    var modPathHeartland = Path.Combine(pathToModHeartlandESM, "BSHeartland.esm");
    var modPathAssets = Path.Combine(pathToModAssetsESM, "BSAssets.esm");

    // List<string?> armors = new();
    List<LocationData> locations = new();
    
    if (File.Exists(modPathHeartland))
    {
        Console.WriteLine(" Mod path exists ");
        using var mod = SkyrimMod.CreateFromBinaryOverlay(modPathHeartland, SkyrimRelease.SkyrimSE);
        // armors = ModHelper.GetEditorIds(ConvertArmorsToMajorRecords(mod.Armors));

        var modsList = env.LoadOrder.PriorityOrder.Select(m => m.Mod).Where(m => m != null).Cast<ISkyrimModGetter>().ToList();
        modsList.Add(mod); // Inject the binary overlay mod into the index list
        
        var combinedCache = new ImmutableLoadOrderLinkCache<ISkyrimMod, ISkyrimModGetter>(modsList, LinkCachePreferences.Default);
        
        locations = modService.GetLocationsFromEsm(mod.Locations, combinedCache).Result;
    }
    else if (File.Exists(modPathAssets))
    {
        Console.WriteLine(" Mod path exists ");
        using var mod = SkyrimMod.CreateFromBinaryOverlay(modPathAssets, SkyrimRelease.SkyrimSE);
        // armors = modService.GetEditorIds(ConvertArmorsToMajorRecords(mod.Armors));

        var modsList = env.LoadOrder.PriorityOrder.Select(m => m.Mod).Where(m => m != null).Cast<ISkyrimModGetter>().ToList();
        modsList.Add(mod); // Inject the binary overlay mod into the index list
        
        var combinedCache = new ImmutableLoadOrderLinkCache<ISkyrimMod, ISkyrimModGetter>(modsList, LinkCachePreferences.Default);
        
        locations = modService.GetLocationsFromEsm(mod.Locations, combinedCache).Result;
    }
    else
    {
        Console.Write($"Warning: Could not find {modPathHeartland} in your Skyrim Data folder.");
    }

    // Return the final data payload back to the browser
    return Results.Ok(new { UniqueLocations = locations });
})
.WithName("GetSkyrimModData");

app.MapGet("/", () => "Hello World!");
app.UseCors("FrontendPolicy");
app.Run();