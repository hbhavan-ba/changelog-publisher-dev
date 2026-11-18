using ChangelogPublisher.Data;
using ChangelogPublisher.Data.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChangelogPublisher.Services;

internal class ConfluenceService : IPublishService
{
    HttpClient HttpClient { get; init; }
    PublisherOptions PublisherOptions { get; init; }
    ILogger<ConfluenceService> Logger { get; init; }

    public ConfluenceService(
        HttpClient httpClient,
        IOptions<PublisherOptions> publisherOptions,
        ILogger<ConfluenceService> logger) =>
        (HttpClient, PublisherOptions, Logger) = 
            (httpClient, publisherOptions.Value, logger);

    public async Task WriteChange(Change change)
    {
        Logger.LogInformation("Writing change to Confluence for space {SpaceId} and parent page {ParentPageId}", PublisherOptions.ConfluenceSpaceId, PublisherOptions.ConfluenceParentPageId);


        Console.WriteLine(JsonSerializer.Serialize(change));
        await MakeConfluenceRequest($"{PublisherOptions.ConfluenceCloudId}/wiki/api/v2/pages", change);
    }

    async Task MakeConfluenceRequest(
        string path,
        Change content)
    {
        var payload = new
        {
            spaceId = PublisherOptions.ConfluenceSpaceId,
            parentId = PublisherOptions.ConfluenceParentPageId,
            status = "current",
            title = content.Title,
            body = new
            {
                representation = "storage",
                value = content.FormatMessage()
            }
        };

        var creds = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{PublisherOptions.ConfluenceEmail}:{PublisherOptions.ConfluenceApiToken}"));

        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.Add("Accept", "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", creds);

        var response = await HttpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }
}
