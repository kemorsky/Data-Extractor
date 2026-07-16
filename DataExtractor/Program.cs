using DataExtractor.Dto;
using System.Text.Json;

var URL = Environment.GetEnvironmentVariable("URL");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// CORS builder services
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.MapOpenApi();

List<LocationDataSheet> locationsCache = new();

var locationsJson = await File.ReadAllTextAsync(
    Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "locations.json"
    )
);

locationsCache = JsonSerializer.Deserialize<List<LocationDataSheet>>(locationsJson) ?? new();

app.MapGet("/locations", async () =>
{
    return Results.Ok(locationsCache);
});

// app.MapGet("/locations", async () =>
// {
//     string filePath = Path.GetTempFileName();
//     using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
//     {
//         await stream.CopyToAsync(fileStream);
//     }

//     Console.WriteLine(filePath);
    
//     var modPathHeartland = Path.Combine(pathToModHeartlandESM, "BSHeartland.esm");
//     var modPathAssets = Path.Combine(pathToModAssetsESM, "BSAssets.esm");
    
//     if (File.Exists(modPathHeartland))
//     {
//         using var env = GameEnvironment.Typical.Skyrim(SkyrimRelease.SkyrimSE);
//         ILinkCache linkCache = env.LinkCache;

//         Console.WriteLine(" Mod path exists ");
//         using var mod = SkyrimMod.CreateFromBinaryOverlay(modPathHeartland, SkyrimRelease.SkyrimSE);

//         var modsList = env.LoadOrder.PriorityOrder.Select(m => m.Mod).Where(m => m != null).Cast<ISkyrimModGetter>().ToList();
//         modsList.Add(mod); // Inject the binary overlay mod into the index list
        
//         var combinedCache = new ImmutableLoadOrderLinkCache<ISkyrimMod, ISkyrimModGetter>(modsList, LinkCachePreferences.Default);
        
//         Console.Write(combinedCache);

//         locationsCache = await dataService.GetLocations(mod.Locations, combinedCache);
//     }
//     // else if (File.Exists(modPathAssets)) // TODO - add support for Assets
//     // {
//     //     Console.WriteLine(" Mod path exists ");
//     //     using var mod = SkyrimMod.CreateFromBinaryOverlay(modPathAssets, SkyrimRelease.SkyrimSE);
//     //     // armors = modService.GetEditorIds(ConvertArmorsToMajorRecords(mod.Armors));

//     //     var modsList = env.LoadOrder.PriorityOrder.Select(m => m.Mod).Where(m => m != null).Cast<ISkyrimModGetter>().ToList();
//     //     modsList.Add(mod); // Inject the binary overlay mod into the index list
        
//     //     var combinedCache = new ImmutableLoadOrderLinkCache<ISkyrimMod, ISkyrimModGetter>(modsList, LinkCachePreferences.Default);
        
//     //     locations = modService.GetLocationsFromEsm(mod.Locations, combinedCache).Result;
//     // }
//     else
//     {
//         Console.Write($"Warning: Could not find {modPathHeartland} in your Skyrim Data folder.");
//         locationsCache = await dataService.GetLocations();
//     }

//     // Return the final data payload back to the browser
//     return Results.Ok(locationsCache);
// })
// .WithName("GetSkyrimModData");

app.MapGet("/locations/{name}", (string name) =>
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

app.MapGet("/locations/filter", (
    string? q,
    string? status,
    string? locationType,
    string? parentLocation,
    string? inhabitants
    ) =>
{
    var statuses = status?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var locationTypes = locationType?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var parentLocations = parentLocation?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var enemies = inhabitants?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    
    var locations = locationsCache
        .Where(x =>
            (statuses == null || statuses.Contains(x.Status, StringComparer.OrdinalIgnoreCase)) &&
            (locationTypes == null || locationTypes.Contains(x.LocationType, StringComparer.OrdinalIgnoreCase)) &&
            (parentLocations == null || parentLocations.Contains(x.ParentLocation, StringComparer.OrdinalIgnoreCase)) &&
            (enemies == null || enemies.Contains(x.Inhabitants, StringComparer.OrdinalIgnoreCase))
        )
        .OrderBy(x => x.ParentLocation ?? string.Empty)
        .ThenBy(x => x.Name)
        .ToList();

        return locations.Count == 0 
            ? Results.NotFound()
            : Results.Ok(locations);
});

app.MapGet("/", () => "Hello World!");
app.Run();