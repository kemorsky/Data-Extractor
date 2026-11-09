using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using DataExtractor.Dto;
using System.Text.Json;
using DataExtractor.Tools;
using Microsoft.AspNetCore.Authentication;

var URL = Environment.GetEnvironmentVariable("URL");
var Issuer = Environment.GetEnvironmentVariable("Issuer");
var ClientId = Environment.GetEnvironmentVariable("ClientId");
var ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
var frontendRedirect = Environment.GetEnvironmentVariable("Frontend-Redirect");

var builder = WebApplication.CreateBuilder(args);

var zitadelIssuer = builder.Configuration["Issuer"]!;
var clientId = builder.Configuration["ClientId"]!;
var clientSecret = builder.Configuration["ClientSecret"]!;

builder.Services.AddOpenApi();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-AppSession";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = zitadelIssuer;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;

        options.SignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;

        options.CallbackPath = "/signin-oidc";

        options.Events.OnRedirectToIdentityProvider = context =>
        {
            Console.WriteLine(
                $"Redirect URI: {context.ProtocolMessage.RedirectUri}",
                $"Redirect URI: {context}"
            );

            return Task.CompletedTask;
        };

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.MapInboundClaims = false;

        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "roles";

        // Testing, temporarily disabled.
        options.SaveTokens = false;
    });

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();
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

app.MapGet("/auth/login", () =>
{
    var properties = new AuthenticationProperties
    {
        RedirectUri = frontendRedirect
    };

    return Results.Challenge(
        properties,
        [OpenIdConnectDefaults.AuthenticationScheme]
    );
});

// Current authenticated user
app.MapGet("/auth/me", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        Id = user.FindFirst("sub")?.Value,
        Name = user.FindFirst("name")?.Value,
        Email = user.FindFirst("email")?.Value
    });
}).RequireAuthorization();

// Logout
app.MapPost("/auth/logout", () =>
{
    var properties = new AuthenticationProperties
    {
        RedirectUri = frontendRedirect
    };

    return Results.SignOut(
        properties,
        [
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        ]
    );
}).RequireAuthorization();

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

public partial class Program { }