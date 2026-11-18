using System.Text.Json.Serialization;

namespace ChangelogPublisher.Models;

public class GitHubCommitResponse
{
    public string Sha { get; set; } = string.Empty;

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; } = string.Empty;

    public GitHubCommit Commit { get; set; } = new();

    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("comments_url")]
    public string CommentsUrl { get; set; } = string.Empty;

    public GitHubUser? Author { get; set; }

    public GitHubUser? Committer { get; set; }

    public List<GitHubCommitParent> Parents { get; set; } = new();

    public GitHubCommitStats Stats { get; set; } = new();

    public List<GitHubCommitFile> Files { get; set; } = new();
}

public class GitHubCommit
{
    public GitHubCommitIdentity Author { get; set; } = new();

    public GitHubCommitIdentity Committer { get; set; } = new();

    public string Message { get; set; } = string.Empty;

    public GitHubGitTree Tree { get; set; } = new();

    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("comment_count")]
    public int CommentCount { get; set; }

    public GitHubCommitVerification Verification { get; set; } = new();
}

public class GitHubCommitIdentity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTimeOffset Date { get; set; }
}

public class GitHubGitTree
{
    public string Sha { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}

public class GitHubCommitVerification
{
    public bool Verified { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? Signature { get; set; }

    public string? Payload { get; set; }

    [JsonPropertyName("verified_at")]
    public DateTimeOffset? VerifiedAt { get; set; }
}

public class GitHubUser
{
    public string Login { get; set; } = string.Empty;

    public long Id { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = string.Empty;

    [JsonPropertyName("gravatar_id")]
    public string GravatarId { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("followers_url")]
    public string FollowersUrl { get; set; } = string.Empty;

    [JsonPropertyName("following_url")]
    public string FollowingUrl { get; set; } = string.Empty;

    [JsonPropertyName("gists_url")]
    public string GistsUrl { get; set; } = string.Empty;

    [JsonPropertyName("starred_url")]
    public string StarredUrl { get; set; } = string.Empty;

    [JsonPropertyName("subscriptions_url")]
    public string SubscriptionsUrl { get; set; } = string.Empty;

    [JsonPropertyName("organizations_url")]
    public string OrganizationsUrl { get; set; } = string.Empty;

    [JsonPropertyName("repos_url")]
    public string ReposUrl { get; set; } = string.Empty;

    [JsonPropertyName("events_url")]
    public string EventsUrl { get; set; } = string.Empty;

    [JsonPropertyName("received_events_url")]
    public string ReceivedEventsUrl { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("user_view_type")]
    public string UserViewType { get; set; } = string.Empty;

    [JsonPropertyName("site_admin")]
    public bool SiteAdmin { get; set; }
}

public class GitHubCommitParent
{
    public string Sha { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
}

public class GitHubCommitStats
{
    public int Total { get; set; }

    public int Additions { get; set; }

    public int Deletions { get; set; }
}

public class GitHubCommitFile
{
    public string Sha { get; set; } = string.Empty;

    public string Filename { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Additions { get; set; }

    public int Deletions { get; set; }

    public int Changes { get; set; }

    [JsonPropertyName("blob_url")]
    public string BlobUrl { get; set; } = string.Empty;

    [JsonPropertyName("raw_url")]
    public string RawUrl { get; set; } = string.Empty;

    [JsonPropertyName("contents_url")]
    public string ContentsUrl { get; set; } = string.Empty;

    public string? Patch { get; set; }
}
