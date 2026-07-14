using System.IO;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;
using DataExtractor.Tool.Services;
using System.Text.Json;

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

Console.WriteLine("Hello, World!");

using var env = GameEnvironment.Typical.Skyrim(SkyrimRelease.SkyrimSE);

if (string.IsNullOrWhiteSpace(pathToModHeartlandESM) || string.IsNullOrWhiteSpace(pathToModAssetsESM))
{
    throw new Exception("DATA-HEARTLANDS is not configured.");
}

var modPathHeartland = Path.Combine(pathToModHeartlandESM, "BSHeartland.esm");
var modPathAssets = Path.Combine(pathToModAssetsESM, "BSAssets.esm");

using var mod = SkyrimMod.CreateFromBinaryOverlay(modPathHeartland, SkyrimRelease.SkyrimSE);
using var mod2 = SkyrimMod.CreateFromBinaryOverlay(modPathAssets, SkyrimRelease.SkyrimSE);

var modsList = env.LoadOrder.PriorityOrder.Select(m => m.Mod).Where(m => m != null).Cast<ISkyrimModGetter>().ToList();
modsList.Add(mod);
modsList.Add(mod2);

var combinedCache = new ImmutableLoadOrderLinkCache<ISkyrimMod, ISkyrimModGetter>(modsList, LinkCachePreferences.Default);

var dataService = new DataService();

// 4. Combine ESM + Google Sheet
var locations =
    await dataService.GetLocations(
        mod.Locations,
        combinedCache);

var outputPath = Path.Combine(
    "..",
    "DataExtractor",
    "Data",
    "locations.json");

// 5. Write JSON
File.WriteAllText(
    outputPath,
    JsonSerializer.Serialize(locations));