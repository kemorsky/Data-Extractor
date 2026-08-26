using DataExtractor.Dto;
using System.Text.Json;
using DataExtractor.Tools;

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
    return locationsCache == null 
            ? Results.NotFound(new
            {
                message = "Locations not found"
            })
            : Results.Ok(locationsCache);
});

static string Slugify(string value)
{
    return value
        .ToLowerInvariant()
        .Trim()
        .Replace("'", "")
        .Replace(" ", "-");
}

app.MapGet("/locations/{slug}", (string slug) =>
{
    var location = locationsCache
        .FirstOrDefault(x => string.Equals(
            Slugify(x.Name),
            slug,
            StringComparison.OrdinalIgnoreCase));

        return location == null 
            ? Results.NotFound()
            : Results.Ok(location);
});

app.MapGet("/locations/filter", (
    string? query,
    string? status,
    string? locationCategory,
    string? locationType,
    string? parentLocation,
    string[]? inhabitants,
    string[]? keywords,
    bool? hasQuest) =>
{
    var searchTerm = query?.Trim();
    var statuses = status?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var locationCategories = locationCategory?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var locationTypes = locationType?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var parentLocations = parentLocation?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var enemies = inhabitants?.Select(e => e.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    var keyWords = keywords?.Select(k => k.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    
    var locations = locationsCache
        .Where(x =>
            (string.IsNullOrWhiteSpace(query) || DataExtractorTools.QueryMatches(x, searchTerm)) &&
            
            (statuses == null || statuses.Contains(x.Status, StringComparer.OrdinalIgnoreCase)) &&
            (locationCategories == null || locationCategories.Contains(x.LocationCategory, StringComparer.OrdinalIgnoreCase)) &&
            (locationTypes == null || locationTypes.Contains(x.LocationType, StringComparer.OrdinalIgnoreCase)) &&
            (parentLocations == null || parentLocations.Contains(x.ParentLocation, StringComparer.OrdinalIgnoreCase)) &&
            (enemies == null || enemies.All(i => i.Any(en => x.Inhabitants.Contains(en, StringComparer.OrdinalIgnoreCase)))) &&
            (keyWords == null || keyWords.All(k => k.Any(kw => x.Keywords.Contains(kw, StringComparer.OrdinalIgnoreCase)))) &&
            (hasQuest != true || x.HasQuest)
        )
        .OrderBy(x => x.ParentLocation ?? string.Empty)
        .ThenBy(x => x.Name)
        .ToList();

        return locations.Count == 0 
            ? Results.NotFound(new
            {
                type = "404",
                message = "No results found"
            })
            : Results.Ok(locations);
});

app.MapGet("/", () => "Hello World!");
app.Run();