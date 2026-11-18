using ChangelogPublisher.Data;
using ChangelogPublisher.Data.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
        Console.WriteLine(JsonSerializer.Serialize(change));
    }
}
