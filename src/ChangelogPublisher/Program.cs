using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ChangelogPublisher.Data.Configuration;
using ChangelogPublisher.Services;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddLogging(opt => opt.AddConsole());

builder.Services.Configure<PublisherOptions>(
    builder.Configuration.GetSection("Publisher")
);

builder.Services.AddHttpClient<IChangeService, GithubService>(
    client =>
    {
        client.BaseAddress = new Uri(
            "https://api.github.com/"
        );

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "ChangelogPublisher/1.0"
        );

        client.DefaultRequestHeaders.Accept.ParseAdd(
            "application/vnd.github+json"
        );
    }
);

builder.Services.AddHttpClient<IPublishService, ConfluenceService>();

builder.Services.AddTransient<IChangelogWriterService, ChangelogWriterService>();

using var host = builder.Build();

var changelogWriter = host.Services
    .GetRequiredService<IChangelogWriterService>();

await changelogWriter.PublishAsync();
