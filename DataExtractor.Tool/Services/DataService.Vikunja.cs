namespace DataExtractor.Tool.Services;

using Mutagen.Bethesda;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using DataExtractor.Tool.Dto;

public class VikunjaServices
{
    public async Task<Dictionary<string, (VikunjaTask Task, string Url)>> LoadVikunjaData()
    {
        // Vikunja Environment Variables
        var vikunjaApiUrl = Environment.GetEnvironmentVariable("VIKUNJA_API_URL");
        var vikunjaApiUrlDungeons = Environment.GetEnvironmentVariable("VIKUNJA_API_URL_DUNGEONS");
        var vikunjaToken = Environment.GetEnvironmentVariable("VIKUNJA_TOKEN");
        var vikunjaFrontendUrl = Environment.GetEnvironmentVariable("VIKUNJA_FRONTEND_URL");
        var vikunjaProjectUrl = Environment.GetEnvironmentVariable("VIKUNJA_PROJECT_URL");

        var lookup = new Dictionary<string, (VikunjaTask Task, string Url)>(StringComparer.OrdinalIgnoreCase);

        // DIAGNOSTIC LOG: Check Environment Variables
        Console.WriteLine($"[Vikunja Diagnostics] URL configured: {!string.IsNullOrEmpty(vikunjaApiUrl)} ('{vikunjaApiUrl}')");
        Console.WriteLine($"[Vikunja Diagnostics] Token configured: {!string.IsNullOrEmpty(vikunjaToken)}");
        
        if (!string.IsNullOrEmpty(vikunjaApiUrl) && !string.IsNullOrEmpty(vikunjaToken))
        {
            var apiUrls = new[] { vikunjaApiUrl, vikunjaApiUrlDungeons }
                .Where(url => !string.IsNullOrEmpty(url))
                .ToList();
            
            using var localHttpClient = new HttpClient();

            foreach (var url in apiUrls)
            {
                try
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", vikunjaToken);
                    httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var httpResponse = await localHttpClient.SendAsync(httpRequest);

                    Console.WriteLine($"[Vikunja Diagnostics] HTTP Status Code Received: {httpResponse.StatusCode} ({(int)httpResponse.StatusCode})");

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var jsonString = await httpResponse.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var responseData = JsonSerializer.Deserialize<VikunjaResponseContainer>(jsonString, options);

                        var tasks = responseData?.Items ?? new List<VikunjaTask>();

                        Console.Write(tasks);
                        Console.WriteLine($"[Vikunja Diagnostics] Extracted {tasks.Count} inner tasks from API payload.");

                        foreach (var task in tasks)
                        {
                            string rawName = task.Name ?? "";
                            string trimmedKey = rawName.Trim();

                            if (rawName.Contains('-'))
                            {
                                var parts = rawName.Split('-', 2);
                                if (parts.Length > 1) 
                                {
                                    trimmedKey = parts[1].Trim();
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(trimmedKey))
                            {
                                var taskUrl = $"{vikunjaFrontendUrl?.TrimEnd('/')}/tasks/{task.Id}";
                                lookup.TryAdd(trimmedKey, (Task: task, Url: taskUrl));
                            }
                        }
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to fetch data from Vikunja API. {ex.Message}");
                }
            }
        }

        return lookup;
    }
}