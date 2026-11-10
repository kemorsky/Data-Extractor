using System.Net;
using DataExtractor.Dto;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using DataExtractor.Tools;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace DataExtractor.Tests;

public class LocationApiTests : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly HttpClient _client;

    public LocationApiTests(WebApplicationFactory<Program> factory)
    {
        var Issuer = Environment.GetEnvironmentVariable("Issuer");
        var ClientId = Environment.GetEnvironmentVariable("ClientId");
        var ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
        var frontendRedirect = Environment.GetEnvironmentVariable("Frontend-Redirect");

        var configuredFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting(
                "ClientId",
                Environment.GetEnvironmentVariable("ClientId")
            );

            builder.UseSetting(
                "ClientSecret",
                Environment.GetEnvironmentVariable("ClientSecret")
            );

            builder.UseSetting(
                "Issuer",
                Environment.GetEnvironmentVariable("Issuer")
            );
        });

        // Creates an in-memory test server hosting the minimal API
        _client = configuredFactory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            }
        );
    }

    [Fact]
    public async Task GetRoot_ReturnsHelloWorld()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello World!", content);
    }

    [Fact]
    public async Task OpenIdConnect_IsRegistered()
    {
        // Act
        var factory = new WebApplicationFactory<Program>();

        using var scope = factory.Services.CreateScope();

        var schemes = scope.ServiceProvider
            .GetRequiredService<IAuthenticationSchemeProvider>();

        var scheme = await schemes.GetSchemeAsync(
            OpenIdConnectDefaults.AuthenticationScheme
        );

        // Assert
        Assert.NotNull(scheme);
    }

    [Fact]
    public async Task SignIn_RedirectsToOpenID()
    {
        // Act
        var response = await _client.GetAsync(
            "/auth/login",
            HttpCompletionOption.ResponseHeadersRead
        );

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var redirect = response.Headers.Location!;

        Assert.Contains("zitadel", redirect.Host);
        Assert.Contains("client_id=", redirect.Query);
        Assert.Contains("response_type=code", redirect.Query);
        Assert.Contains("redirect_uri=", redirect.Query);
    }
        

    [Fact]
    public async Task GetLocations_ReturnsSuccessAndData()
    {
        // Act
        var response = await _client.GetAsync("/locations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var locations = await response.Content.ReadFromJsonAsync<List<LocationDataSheet>>();
        Assert.NotNull(locations);
    }

    [Fact]
    public async Task GetFilteredLocations_WithQuestFilter_ReturnsFilteredResults()
    {
        // Act: append query parameters to the URL
        var response = await _client.GetAsync("/locations/filter?hasQuest=true");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var locations = await response.Content.ReadFromJsonAsync<List<LocationDataSheet>>();
            Assert.NotNull(locations);
            Assert.All(locations, loc => Assert.True(loc.HasQuest));
        }
        else
        {
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}