using System;
using System.Collections.Generic;
using System.Text;

namespace ChangelogPublisher.Constants;

internal class EnvironmentVariables
{
    internal class GitHub
    {
        internal const string GH_TOKEN = "GH_TOKEN";
        internal const string REPOSITORY = "REPOSITORY";
        internal const string COMMIT_SHA = "COMMIT_SHA";
        internal const string BEFORE_SHA = "BEFORE_SHA";
    }
    internal class  Confluence
    {
        internal const string CONFLUENCE_CLOUD_ID = "CONFLUENCE_CLOUD_ID";
        internal const string CONFLUENCE_SPACE_ID = "CONFLUENCE_SPACE_ID";
        internal const string CONFLUENCE_PARENT_PAGE_ID = "CONFLUENCE_PARENT_PAGE_ID";
        internal const string CONFLUENCE_EMAIL = "CONFLUENCE_EMAIL";
        internal const string CONFLUENCE_API_TOKEN = "CONFLUENCE_API_TOKEN";
    }
}
