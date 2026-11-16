import os
import re
import time
import html
from urllib.parse import urljoin

import requests

# --------------------------------------------------
# Get environment variables
# --------------------------------------------------

REPO = os.environ["REPOSITORY"]
COMMIT_SHA = int(os.environ["COMMIT_SHA"])
GH_TOKEN = os.environ["GH_TOKEN"]

BASE_URL = os.environ["CONFLUENCE_URL"].rstrip("/")
SPACE_ID = os.environ["CONFLUENCE_SPACE_ID"]
PARENT_ID = os.environ["CONFLUENCE_PARENT_PAGE_ID"]

AUTH = (
    os.environ["CONFLUENCE_EMAIL"],
    os.environ["CONFLUENCE_API_TOKEN"],
)

API = f"{BASE_URL}/wiki/api/v2"

SESSION = requests.Session()
SESSION.headers.update({
    "Accept": "application/json"
})

GH_HEADERS = {
    "Authorization": f"Bearer {GH_TOKEN}",
    "Accept": "application/vnd.github+json",
    "X-GitHub-Api-Version": "2022-11-28",
}

MAX_RETRIES = 5

# --------------------------------------------------
# HTTP helpers
# --------------------------------------------------

def github_get(path, params=None):
    url = f"https://api.github.com/repos/{REPO}/{path}"

    response = SESSION.get(
        url,
        headers=GH_HEADERS,
        params=params,
        timeout=30,
    )
    response.raise_for_status()
    return response.json()


def confluence_request(method, path, **kwargs):
    url = path if path.startswith("http") else API + path

    response = SESSION.request(
        method,
        url,
        auth=AUTH,
        timeout=30,
        **kwargs,
    )

    # Caller handles optimistic-lock conflicts.
    if response.status_code in (409, 412):
        return response

    response.raise_for_status()
    return response


# --------------------------------------------------
# GitHub data
# --------------------------------------------------

def get_pull_request():
    pr = github_get(f"commits/{COMMIT_SHA}/pulls")

    if not pr["merged"]:
        raise ValueError("Pull request was not merged")

    if pr["base"]["ref"] != "main":
        raise ValueError("Target branch is not main")

    return pr


def get_commits():
    commits = []
    page = 1

    while True:
        batch = github_get(
            f"pulls/{PR_NUMBER}/commits",
            {"per_page": 100, "page": page},
        )

        commits.extend(batch)

        if len(batch) < 100:
            break

        page += 1

    return commits


# --------------------------------------------------
# Confluence page lookup
# --------------------------------------------------

def find_existing_page(title):
    """
    Find a page by exact title, space, and parent.
    """

    path = "/pages"
    params = {
        "space-id": SPACE_ID,
        "title": title,
        "limit": 100,
    }

    while path:
        response = confluence_request(
            "GET", path, params=params
        )
        data = response.json()

        for page in data.get("results", []):
            if (
                page.get("title") == title
                and str(page.get("parentId")) == str(PARENT_ID)
                and str(page.get("spaceId")) == str(SPACE_ID)
            ):
                return page

        next_link = data.get("_links", {}).get("next")

        if next_link:
            path = urljoin(BASE_URL, next_link)
            params = None
        else:
            path = None

    return None


def get_page(page_id):
    response = confluence_request(
        "GET",
        f"/pages/{page_id}",
        params={"body-format": "storage"},
    )
    return response.json()


# --------------------------------------------------
# HTML generation
# --------------------------------------------------

def escape(value):
    return html.escape(str(value), quote=True)


def build_merge_entry(pr, commits):
    """
    Generate one identifiable merge-history entry.
    """

    marker = f"{REPO}#{PR_NUMBER}"

    commit_items = []

    for commit in commits:
        sha = commit["sha"]
        message = commit["commit"]["message"]

        url = (
            f"https://github.com/{REPO}/commit/{sha}"
        )

        commit_items.append(
            "<li>"
            f'<a href="{escape(url)}">'
            f"{escape(sha[:7])}</a>"
            f"<pre>{escape(message)}</pre>"
            "</li>"
        )

    commit_html = "".join(commit_items)

    merged_by = (pr.get("merged_by") or {}).get(
        "login", "Unknown"
    )

    description = pr.get("body") or "No description"

    return f"""
    <h2>Merge #{PR_NUMBER} - {escape(pr["merged_at"])}</h2>

    <p>
      <strong>GitHub PR:</strong>
      {escape(marker)}
    </p>

    <p>
      <strong>Merged By:</strong>
      {escape(merged_by)}
    </p>

    <p>
      <strong>Pull Request:</strong>
      <a href="{escape(pr["html_url"])}">
        #{PR_NUMBER}
      </a>
    </p>

    <h3>Pull Request Description</h3>
    <pre>{escape(description)}</pre>

    <h3>Commits</h3>
    <ul>{commit_html}</ul>

    <hr />
    """


def entry_exists(body):
    """
    Check for this PR's unique marker.

    The marker is stored as text in the page body.
    """

    marker = f"{REPO}#{PR_NUMBER}"

    pattern = (
        r"<strong>GitHub PR:</strong>\s*"
        + re.escape(escape(marker))
    )

    return re.search(pattern, body) is not None


def initial_body(branch, merge_entry):
    return f"""
    <h1>Change History</h1>

    <p>
      <strong>Source Branch:</strong>
      {escape(branch)}
    </p>

    <p>
      <strong>GitHub Repository:</strong>
      {escape(REPO)}
    </p>

    <h2>Merge History</h2>

    {merge_entry}
    """


# --------------------------------------------------
# Create or update
# --------------------------------------------------

def create_page(title, body):
    payload = {
        "spaceId": SPACE_ID,
        "status": "current",
        "title": title,
        "parentId": PARENT_ID,
        "body": {
            "representation": "storage",
            "value": body,
        },
    }

    return confluence_request(
        "POST",
        "/pages",
        json=payload,
    )


def update_page(page, new_body):
    page_id = page["id"]

    version = page["version"]["number"]

    payload = {
        "id": page_id,
        "status": "current",
        "title": page["title"],
        "body": {
            "representation": "storage",
            "value": new_body,
        },
        "version": {
            "number": version + 1,
            "message": f"GitHub merge #{PR_NUMBER}",
        },
    }

    return confluence_request(
        "PUT",
        f"/pages/{page_id}",
        json=payload,
    )


def publish(pr, commits):
    branch = pr["head"]["ref"]
    title = branch

    merge_entry = build_merge_entry(pr, commits)

    for attempt in range(MAX_RETRIES):

        existing = find_existing_page(title)

        # ------------------------------------------
        # New branch: create page
        # ------------------------------------------

        if existing is None:
            body = initial_body(branch, merge_entry)

            try:
                response = create_page(title, body)

                if response.status_code in (409, 412):
                    raise RuntimeError(
                        "Concurrent page creation"
                    )

                page_id = response.json()["id"]

                print(
                    f"Created Confluence page {page_id}"
                )
                return

            except (requests.HTTPError, RuntimeError):
                # A concurrent workflow may have
                # created the page first.
                if find_existing_page(title) is None:
                    raise

        # ------------------------------------------
        # Existing branch: append merge history
        # ------------------------------------------

        existing = find_existing_page(title)

        if existing is None:
            time.sleep(2)
            continue

        page = get_page(existing["id"])

        current_body = (
            page.get("body", {})
                .get("storage", {})
                .get("value", "")
        )

        # Prevent duplicate PR entries.
        if entry_exists(current_body):
            print(
                f"PR #{PR_NUMBER} already published. "
                "Skipping."
            )
            return

        updated_body = current_body + merge_entry

        response = update_page(page, updated_body)

        if response.status_code in (409, 412):
            print("Version conflict. Retrying...")
            time.sleep(2 * (attempt + 1))
            continue

        print(
            f"Updated Confluence page {page['id']}"
        )
        return

    raise RuntimeError(
        "Unable to publish after multiple retries"
    )


# --------------------------------------------------
# Entry point
# --------------------------------------------------

def main():
    pr = get_pull_request()
    commits = get_commits()

    publish(pr, commits)


if __name__ == "__main__":
    main()
