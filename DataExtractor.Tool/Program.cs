using System.IO;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Analyzers;
using Mutagen.Bethesda.Analyzers.Skyrim;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using DataExtractor.Tool.Services;
using System.Text.Json;
using System.Reflection;

var credentialPath = Path.Combine(
    AppContext.BaseDirectory,
    "utils",
    "service-account.json");

Environment.SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    credentialPath);

// var pathToModHeartlandESM = Environment.GetEnvironmentVariable("DATA_HEARTLANDS");
// var pathToModAssetsESM = Environment.GetEnvironmentVariable("DATA_ASSETS");
var URL = Environment.GetEnvironmentVariable("URL");

var pluginDirectory = Path.Combine(
    Directory.GetCurrentDirectory(),
    "plugins");

var modPathHeartland = Path.Combine(
    pluginDirectory,
    "BSHeartland.esm");

var modPathAssets = Path.Combine(
    pluginDirectory,
    "BSAssets.esm");

if (!File.Exists(modPathHeartland))
{
    throw new FileNotFoundException(
        $"BSHeartland.esm not found: {modPathHeartland}");
}

if (!File.Exists(modPathAssets))
{
    throw new FileNotFoundException(
        $"BSAssets.esm not found: {modPathAssets}");
}

Console.WriteLine("Hello, World!");

// using var env = GameEnvironment.Typical.Skyrim(SkyrimRelease.SkyrimSE);

// using var env = GameEnvironment.Typical
//     .Builder<ISkyrimMod, ISkyrimModGetter>(GameRelease.SkyrimSE)
//     .WithTargetDataFolder(pluginDirectory)
//     .Build();

var loadOrder = LoadOrder.Import<ISkyrimModGetter>(
    pluginDirectory,
    GameRelease.SkyrimSE);

if (string.IsNullOrWhiteSpace(modPathHeartland) || string.IsNullOrWhiteSpace(modPathAssets))
{
    throw new Exception("DATA-HEARTLANDS is not configured.");
}

// if (string.IsNullOrWhiteSpace(pathToModHeartlandESM) || string.IsNullOrWhiteSpace(pathToModAssetsESM))
// {
//     throw new Exception("DATA-HEARTLANDS is not configured.");
// }

// var modPathHeartland = Path.Combine(pathToModHeartlandESM, "BSHeartland.esm");
// var modPathAssets = Path.Combine(pathToModAssetsESM, "BSAssets.esm");

using var mod = SkyrimMod.CreateFromBinaryOverlay(modPathHeartland, SkyrimRelease.SkyrimSE);
using var mod2 = SkyrimMod.CreateFromBinaryOverlay(modPathAssets, SkyrimRelease.SkyrimSE);

List<ICellGetter> cells = mod
    .EnumerateMajorRecords<ICellGetter>()
    .Concat(mod2.EnumerateMajorRecords<ICellGetter>())
    .ToList();

var modsList = loadOrder.PriorityOrder.Select(m => m.Mod).Where(m => m != null).Cast<ISkyrimModGetter>().ToList();
modsList.Add(mod2);
modsList.Add(mod);

Console.WriteLine($"Heartlands: {mod.ModKey}");

foreach (var master in mod.ModHeader.MasterReferences)
{
    Console.WriteLine($"  Master: {master.Master.FileName}");
}

Console.WriteLine($"Assets: {mod2.ModKey}");

foreach (var master in mod2.ModHeader.MasterReferences)
{
    Console.WriteLine($"  Master: {master.Master.FileName}");
}

var combinedCache = new ImmutableLoadOrderLinkCache<ISkyrimMod, ISkyrimModGetter>(modsList, LinkCachePreferences.Default);

var googleServices = new GoogleServices();
var vikunjaServices = new VikunjaServices();
var lootServices = new LootServices();
var dataService = new DataService(googleServices, vikunjaServices, lootServices);

// 4. Combine ESM + Google Sheet
var locations =
    await dataService.GetLocations(
        mod.Locations,
        combinedCache,
        cells);

var outputPath = Path.Combine(
    "..",
    "DataExtractor",
    "Data",
    "locations.json");

// 5. Write JSON
var json = JsonSerializer.Serialize(
    locations,
    new JsonSerializerOptions
    {
        WriteIndented = true
    });

File.WriteAllText(outputPath, json);