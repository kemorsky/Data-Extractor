using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using DataExtractor.Dto;
using System.Text.Json;
using DataExtractor.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var zitadelIssuer = builder.Configuration["ISSUER"];
var clientId = builder.Configuration["CLIENT_ID"];
var clientSecret = builder.Configuration["CLIENT_SECRET"];
var frontendRedirect = builder.Configuration["FRONTEND_REDIRECT"];
var frontendOrigin = builder.Configuration["URL"] ?? "http://localhost:5173";

builder.Services.AddOpenApi();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultSignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Secure-AppSession";
        options.Cookie.Domain = "library.beyondskyrim.org";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
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

        options.NonceCookie.SameSite = SameSiteMode.Lax;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;

        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

        options.Events.OnRedirectToIdentityProvider = context =>
        {
            Console.WriteLine($"Request Scheme: {context.Request.Scheme}");
            Console.WriteLine($"Request Host: {context.Request.Host}");
            Console.WriteLine($"Redirect URI: {context.ProtocolMessage.RedirectUri}");

            return Task.CompletedTask;
        };

        options.Events.OnTokenValidated = context =>
        {
            Console.WriteLine("OIDC TOKEN VALIDATED");
            Console.WriteLine($"Subject: {context.Principal?.FindFirst("sub")?.Value}");

            return Task.CompletedTask;
        };

        options.Events.OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"OIDC AUTHENTICATION FAILED: {context.Exception}");
            return Task.CompletedTask;
        };

        options.Events.OnTicketReceived = context =>
        {
            Console.WriteLine("AUTHENTICATION TICKET RECEIVED");
            Console.WriteLine($"Redirect: {context.ReturnUri}");

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
        policy.WithOrigins(frontendOrigin)
        //policy.WithOrigins("http://localhost:5173")
        // policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
            ?.Error;

        Console.WriteLine(exception);

        context.Response.StatusCode = 500;

        await context.Response.WriteAsJsonAsync(new
        {
            message = "Internal server error"
        });
    });
});

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

app.MapGet("/", () => "Hello World!");

app.MapGet("/auth/signin", () =>
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
app.MapPost("/auth/signout", () =>
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

app.Run();

public partial class Program { }