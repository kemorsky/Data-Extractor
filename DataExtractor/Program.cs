using DataExtractor.Dto;
using System.Text.Json;

// Retrieve credentials
// var credentialPath = Path.Combine(
//     AppContext.BaseDirectory,
//     "utils",
//     "service-account.json");

// Environment.SetEnvironmentVariable(
//     "GOOGLE_APPLICATION_CREDENTIALS",
//     credentialPath);

// var pathToModHeartlandESM = Environment.GetEnvironmentVariable("DATA-HEARTLANDS");
// var pathToModAssetsESM = Environment.GetEnvironmentVariable("DATA-ASSETS");
var URL = Environment.GetEnvironmentVariable("URL");

// Console.Write(pathToModHeartlandESM);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Services builder
// builder.Services.AddScoped<IDataService, DataService>();
// builder.Services.AddScoped<DownloadEsm>();

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

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.MapOpenApi();

List<LocationDataSheet> locationsCache = new();

// var dataService = new DataService();
// var downloadEsm = new DownloadEsm();

// var stream = downloadEsm.DownloadFromDrive();

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

app.MapGet("/", () => "Hello World!");
app.Run();