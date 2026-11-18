using dotenv.net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ChangelogPublisher.Data.Configuration;
using ChangelogPublisher.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


var builder = Host.CreateApplicationBuilder(args);

DotEnv.Load();

builder.Configuration
    .AddEnvironmentVariables();

/// Load environment variables from .env file for local development
//if (builder.Environment.IsDevelopment())

builder.Services.AddLogging(opt => opt.AddConsole());

builder.Services.AddSingleton(Options.Create(PublisherOptionsImpl.LoadFromEnvironment()));

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
