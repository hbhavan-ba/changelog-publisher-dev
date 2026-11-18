namespace ChangelogPublisher.Data.Configuration;

public sealed class PublisherOptions
{
    public required string Repository {get; init; }
    public required string CommitSHA { get; init; }
    public required string TargetBranch { get; init; } = "main";

    public required string GitHubToken { get; init; }

    public required string ConfluenceCloudId { get; init; }
    public required string ConfluenceSpaceId { get; init; }
    public required string ConfluenceParentPageId { get; init; }
    public required string ConfluenceEmail { get; init; }
    public required string ConfluenceApiToken { get; init; }
}

internal static class PublisherOptionsImpl
{
    internal static PublisherOptions LoadFromEnvironment()
    {
        static string env(string s) => Environment.GetEnvironmentVariable(s)
            ?? throw new InvalidOperationException($"Environment variable {s} is missing");

        return new PublisherOptions
        {
            Repository = env(Constants.EnvironmentVariables.GitHub.REPOSITORY),
            CommitSHA = env(Constants.EnvironmentVariables.GitHub.COMMIT_SHA),
            TargetBranch = "main",
            GitHubToken = env(Constants.EnvironmentVariables.GitHub.GH_TOKEN),
            ConfluenceCloudId = env(Constants.EnvironmentVariables.Confluence.CONFLUENCE_CLOUD_ID),
            ConfluenceSpaceId = env(Constants.EnvironmentVariables.Confluence.CONFLUENCE_SPACE_ID),
            ConfluenceParentPageId = env(Constants.EnvironmentVariables.Confluence.CONFLUENCE_PARENT_PAGE_ID),
            ConfluenceEmail = env(Constants.EnvironmentVariables.Confluence.CONFLUENCE_EMAIL),
            ConfluenceApiToken = env(Constants.EnvironmentVariables.Confluence.CONFLUENCE_API_TOKEN)
        };
    }

    extension(PublisherOptions options)
    {
        internal string GetConfluenceAuthHeader()
        {
            var authString = $"{options.ConfluenceEmail}:{options.ConfluenceApiToken}";
            var authBytes = System.Text.Encoding.UTF8.GetBytes(authString);
            var authBase64 = Convert.ToBase64String(authBytes);
            return $"Basic {authBase64}";
        }
    }
}
