using System.IO;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;
using DataExtractor.Services;
using DataExtractor.Dto;

// Retrieve credentials
var credentialPath = Path.Combine(
    AppContext.BaseDirectory,
    "utils",
    "service-account.json");

Environment.SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    credentialPath);

var pathToModHeartlandESM = Environment.GetEnvironmentVariable("DATA-HEARTLANDS");
var pathToModAssetsESM = Environment.GetEnvironmentVariable("DATA-ASSETS");
var URL = Environment.GetEnvironmentVariable("URL");

Console.Write(pathToModHeartlandESM);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Services builder
builder.Services.AddScoped<IDataService, DataService>();

// CORS builder services
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        if (URL != null)
        {
            policy.WithOrigins("http://localhost:5173", URL)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.MapOpenApi();

List<LocationDataSheet> locationsCache = new();

var dataService = new DataService();

app.MapGet("/locations", async () =>
{
    using var env = GameEnvironment.Typical.Skyrim(SkyrimRelease.SkyrimSE);
    ILinkCache linkCache = env.LinkCache;

    // var weapons = ModHelper.GetEditorIds(env.LoadOrder.PriorityOrder.Weapon().WinningOverrides());

    var modPathHeartland = Path.Combine(pathToModHeartlandESM, "BSHeartland.esm");
    var modPathAssets = Path.Combine(pathToModAssetsESM, "BSAssets.esm");
    
    if (File.Exists(modPathHeartland))
    {
        Console.WriteLine(" Mod path exists ");
        using var mod = SkyrimMod.CreateFromBinaryOverlay(modPathHeartland, SkyrimRelease.SkyrimSE);
        // armors = ModHelper.GetEditorIds(ConvertArmorsToMajorRecords(mod.Armors));

        var modsList = env.LoadOrder.PriorityOrder.Select(m => m.Mod).Where(m => m != null).Cast<ISkyrimModGetter>().ToList();
        modsList.Add(mod); // Inject the binary overlay mod into the index list
        
        var combinedCache = new ImmutableLoadOrderLinkCache<ISkyrimMod, ISkyrimModGetter>(modsList, LinkCachePreferences.Default);
        
        Console.Write(combinedCache);

        locationsCache = await dataService.GetLocations(mod.Locations, combinedCache);
    }
    // else if (File.Exists(modPathAssets)) // TODO - add support for Assets
    // {
    //     Console.WriteLine(" Mod path exists ");
    //     using var mod = SkyrimMod.CreateFromBinaryOverlay(modPathAssets, SkyrimRelease.SkyrimSE);
    //     // armors = modService.GetEditorIds(ConvertArmorsToMajorRecords(mod.Armors));

    //     var modsList = env.LoadOrder.PriorityOrder.Select(m => m.Mod).Where(m => m != null).Cast<ISkyrimModGetter>().ToList();
    //     modsList.Add(mod); // Inject the binary overlay mod into the index list
        
    //     var combinedCache = new ImmutableLoadOrderLinkCache<ISkyrimMod, ISkyrimModGetter>(modsList, LinkCachePreferences.Default);
        
    //     locations = modService.GetLocationsFromEsm(mod.Locations, combinedCache).Result;
    // }
    else
    {
        Console.Write($"Warning: Could not find {modPathHeartland} in your Skyrim Data folder.");
        locationsCache = await dataService.GetLocations();
    }

    // Return the final data payload back to the browser
    return Results.Ok(locationsCache);
})
.WithName("GetSkyrimModData");

// Google Sheets endpoint
// app.MapGet("/test", async () =>
// {
//     locationsCache = await sheetService.GetLocationsFromSheets();

//     return Results.Ok(locationsCache);
    
// });

// Google Sheets endpoint
app.MapGet("/locations/{name}", async (string name) =>
{
    name = name.Replace("-", " ");

    var location = locationsCache
        .FirstOrDefault(x => string.Equals(
                x.Name,
                name,
                StringComparison.OrdinalIgnoreCase));

        return location == null 
            ? Results.NotFound()
            : Results.Ok(location);
});

app.MapGet("/", () => "Hello World!");
app.UseCors("FrontendPolicy");
app.Run();