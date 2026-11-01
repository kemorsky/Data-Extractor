using System.Net;
using DataExtractor.Dto;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using DataExtractor.Tools;
using System.Net.Http.Json;

namespace DataExtractor.Tests;

public class LocationApiTests : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly HttpClient _client;

    public LocationApiTests(WebApplicationFactory<Program> factory)
    {
        // Creates an in-memory test server hosting the minimal API
        _client = factory.CreateClient();
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