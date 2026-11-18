using ChangelogPublisher.Data;
using ChangelogPublisher.Data.Configuration;
using ChangelogPublisher.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChangelogPublisher.Services;

internal class GithubService : IChangeService
{
    JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    HttpClient HttpClient { get; init; }
    PublisherOptions PublisherOptions { get; init; }

    public GithubService(
        HttpClient httpClient,
        IOptions<PublisherOptions> publisherOptions) =>
        (HttpClient, PublisherOptions) = 
            (httpClient, publisherOptions.Value);

    public async Task<Change> GetChange()
    {
        var pullRequest = await GetPullRequest();

        var title = pullRequest.Commit.Message
            .Split('\n')
            .LastOrDefault(pullRequest.Commit.Message);

        var details = pullRequest.Parents
            .Select(async parent => await MakeGithubRequest<GitHubCommitResponse>(parent.Url))
            .Select(x => x.Result.Commit.Message);

        return new Change(
            pullRequest.Commit.Committer.Date.Date,
            pullRequest.Commit.Author.Email,
            title,
            "Body",
            details);
    }

    internal async Task<GitHubCommitResponse> GetPullRequest()
    {
        var repository = PublisherOptions.Repository;
        var commitSHA = PublisherOptions.CommitSHA;

        return await MakeGithubRequest<GitHubCommitResponse>($"/repos/{repository}/commits/{commitSHA}");
    }

    async Task<T> MakeGithubRequest<T>(string path)
    {
        var response = await HttpClient.GetAsync(path);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Request to {path} failed with status code {response.StatusCode}: {content}");

        return JsonSerializer.Deserialize<T>(content, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize response from {path}");
    }
}
