// Program.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Sleet.Search;
using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Services
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP Request Pipeline (Middleware)
if (app.Environment.IsDevelopment())
{
    // ... Swagger config placeholder
}

// Map API routes for traditional controllers (if you implement them later)
app.MapControllers();

// --- NEW: Converted Search API Endpoint ---
// Route: /search/source/{source}/query
app.MapGet("search/source/{source}/query", async (string source, [FromQuery] string q, [FromQuery] string skip,
    [FromQuery] string take, [FromQuery] bool? prerelease) =>
{
// --- Startup Logic Mapping from the old function ---

    var sourceUrl = new System.Uri(System.Web.HttpUtility.UrlDecode(source));

// Handle default values and parsing for query parameters
    var searchQuery = q; // Query parameter 'q'
    int skipCount = 0;
    int takeCount = 100;
    bool isPrerelease = prerelease ?? false;

    if (int.TryParse(skip, out var skipNum) && skipNum >= 0)
    {
        skipCount = skipNum;
    }

    if (int.TryParse(take, out var takeNum) && takeNum > 0)
    {
        takeCount = takeNum;
    }

// Use a using statement for better resource management if possible
    using (var httpClient = new HttpClient())
    {
        try
        {
            // Read static search results (Flurl replacement needed here, using HttpClient standard)
            HttpResponseMessage response = await httpClient.GetAsync(sourceUrl);

            if (response.IsSuccessStatusCode)
            {
                // Convert content stream to string/byte array for the utility method
                var jsonStream = await response.Content.ReadAsStreamAsync();

                try
                {
                    // Parse static search results
                    // NOTE: Assuming SearchUtils.ReadJson accepts a Stream or MemoryStream now.
                    var json = await SearchUtils.ReadJson(jsonStream);

                    var entries = SearchUtils.GetEntries(json);
                    var terms = SearchUtils.GetTerms(searchQuery);

                    // Filter on prerel and modify results to remove prerel packages
                    entries = SearchUtils.FilterOnPreRel(entries, isPrerelease);

                    // Filter on search terms if given
                    var searchResults = SearchUtils.SearchEntries(entries, terms);

                    // Apply skip/take
                    var toAdd = searchResults.Skip(skipCount).Take(takeCount).ToList();

                    // Build response
                    var result = SearchUtils.GetSearchResult(toAdd, searchResults.Count, (JObject)json["@context"]);

                    //var resString = result.ToString();
                    var jsonSerializer = new Newtonsoft.Json.JsonSerializerSettings { Formatting = Newtonsoft.Json.Formatting.None };
                    var resString = JsonConvert.SerializeObject(result, jsonSerializer);

                    //return Results.Ok(resString); // Use standard Minimal API results pattern
                    return Results.Content(resString, "application/json");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing JSON: {ex}");
                    return Results.StatusCode(500);
                }
            }

            // If the HTTP call failed, return the status code received.
            return Results.StatusCode((int)response.StatusCode);
        }
        catch (HttpRequestException hrex)
        {
            Console.WriteLine($"Network Error: {hrex}");
            return Results.StatusCode(503); // Service Unavailable for connection errors
        }
    }
});


// 4. Run the Application
await app.RunAsync();