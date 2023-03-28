#!/usr/bin/env -S dotnet fsi

open System
open System.IO
open System.Net.Http
open System.Net.Http.Headers

#r "nuget: FSharp.Data, Version=5.0.2"
open FSharp.Data

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"
open Fsdk

let githubEventPath = Environment.GetEnvironmentVariable "GITHUB_EVENT_PATH"

if String.IsNullOrEmpty githubEventPath then
    Console.Error.WriteLine
        "This script is meant to be used only within a GitHubCI pipeline"

    Environment.Exit 2

type githubEventType =
    JsonProvider<"""
{
    "action": "opened",
    "number": 4,
    "pull_request": {
      "_links": {
        "comments": {
          "href": "https://api.github.com/repos/realmarv/conventions/issues/4/comments"
        },
        "commits": {
          "href": "https://api.github.com/repos/realmarv/conventions/pulls/4/commits"
        },
        "html": {
          "href": "https://github.com/realmarv/conventions/pull/4"
        },
        "issue": {
          "href": "https://api.github.com/repos/realmarv/conventions/issues/4"
        },
        "review_comment": {
          "href": "https://api.github.com/repos/realmarv/conventions/pulls/comments{/number}"
        },
        "review_comments": {
          "href": "https://api.github.com/repos/realmarv/conventions/pulls/4/comments"
        },
        "self": {
          "href": "https://api.github.com/repos/realmarv/conventions/pulls/4"
        },
        "statuses": {
          "href": "https://api.github.com/repos/realmarv/conventions/statuses/c5eb50ea6c5a72d61ab85d96ffc66cc4741d3133"
        }
      },
      "active_lock_reason": null,
      "additions": 21,
      "assignee": null,
      "assignees": [],
      "author_association": "OWNER",
      "auto_merge": null,
      "base": {
        "label": "realmarv:master",
        "ref": "master",
        "repo": {
          "allow_auto_merge": false,
          "allow_forking": true,
          "allow_merge_commit": true,
          "allow_rebase_merge": true,
          "allow_squash_merge": true,
          "allow_update_branch": false,
          "archive_url": "https://api.github.com/repos/realmarv/conventions/{archive_format}{/ref}",
          "archived": false,
          "assignees_url": "https://api.github.com/repos/realmarv/conventions/assignees{/user}",
          "blobs_url": "https://api.github.com/repos/realmarv/conventions/git/blobs{/sha}",
          "branches_url": "https://api.github.com/repos/realmarv/conventions/branches{/branch}",
          "clone_url": "https://github.com/realmarv/conventions.git",
          "collaborators_url": "https://api.github.com/repos/realmarv/conventions/collaborators{/collaborator}",
          "comments_url": "https://api.github.com/repos/realmarv/conventions/comments{/number}",
          "commits_url": "https://api.github.com/repos/realmarv/conventions/commits{/sha}",
          "compare_url": "https://api.github.com/repos/realmarv/conventions/compare/{base}...{head}",
          "contents_url": "https://api.github.com/repos/realmarv/conventions/contents/{+path}",
          "contributors_url": "https://api.github.com/repos/realmarv/conventions/contributors",
          "created_at": "2022-10-12T08:48:57Z",
          "default_branch": "master",
          "delete_branch_on_merge": false,
          "deployments_url": "https://api.github.com/repos/realmarv/conventions/deployments",
          "description": null,
          "disabled": false,
          "downloads_url": "https://api.github.com/repos/realmarv/conventions/downloads",
          "events_url": "https://api.github.com/repos/realmarv/conventions/events",
          "fork": true,
          "forks": 0,
          "forks_count": 0,
          "forks_url": "https://api.github.com/repos/realmarv/conventions/forks",
          "full_name": "realmarv/conventions",
          "git_commits_url": "https://api.github.com/repos/realmarv/conventions/git/commits{/sha}",
          "git_refs_url": "https://api.github.com/repos/realmarv/conventions/git/refs{/sha}",
          "git_tags_url": "https://api.github.com/repos/realmarv/conventions/git/tags{/sha}",
          "git_url": "git://github.com/realmarv/conventions.git",
          "has_discussions": false,
          "has_downloads": true,
          "has_issues": false,
          "has_pages": false,
          "has_projects": true,
          "has_wiki": true,
          "homepage": null,
          "hooks_url": "https://api.github.com/repos/realmarv/conventions/hooks",
          "html_url": "https://github.com/realmarv/conventions",
          "id": 550128772,
          "is_template": false,
          "issue_comment_url": "https://api.github.com/repos/realmarv/conventions/issues/comments{/number}",
          "issue_events_url": "https://api.github.com/repos/realmarv/conventions/issues/events{/number}",
          "issues_url": "https://api.github.com/repos/realmarv/conventions/issues{/number}",
          "keys_url": "https://api.github.com/repos/realmarv/conventions/keys{/key_id}",
          "labels_url": "https://api.github.com/repos/realmarv/conventions/labels{/name}",
          "language": "TypeScript",
          "languages_url": "https://api.github.com/repos/realmarv/conventions/languages",
          "license": {
            "key": "mit",
            "name": "MIT License",
            "node_id": "MDc6TGljZW5zZTEz",
            "spdx_id": "MIT",
            "url": "https://api.github.com/licenses/mit"
          },
          "merge_commit_message": "PR_TITLE",
          "merge_commit_title": "MERGE_MESSAGE",
          "merges_url": "https://api.github.com/repos/realmarv/conventions/merges",
          "milestones_url": "https://api.github.com/repos/realmarv/conventions/milestones{/number}",
          "mirror_url": null,
          "name": "conventions",
          "node_id": "R_kgDOIMpMhA",
          "notifications_url": "https://api.github.com/repos/realmarv/conventions/notifications{?since,all,participating}",
          "open_issues": 4,
          "open_issues_count": 4,
          "owner": {
            "avatar_url": "https://avatars.githubusercontent.com/u/50144546?v=4",
            "events_url": "https://api.github.com/users/realmarv/events{/privacy}",
            "followers_url": "https://api.github.com/users/realmarv/followers",
            "following_url": "https://api.github.com/users/realmarv/following{/other_user}",
            "gists_url": "https://api.github.com/users/realmarv/gists{/gist_id}",
            "gravatar_id": "",
            "html_url": "https://github.com/realmarv",
            "id": 50144546,
            "login": "realmarv",
            "node_id": "MDQ6VXNlcjUwMTQ0NTQ2",
            "organizations_url": "https://api.github.com/users/realmarv/orgs",
            "received_events_url": "https://api.github.com/users/realmarv/received_events",
            "repos_url": "https://api.github.com/users/realmarv/repos",
            "site_admin": false,
            "starred_url": "https://api.github.com/users/realmarv/starred{/owner}{/repo}",
            "subscriptions_url": "https://api.github.com/users/realmarv/subscriptions",
            "type": "User",
            "url": "https://api.github.com/users/realmarv"
          },
          "private": false,
          "pulls_url": "https://api.github.com/repos/realmarv/conventions/pulls{/number}",
          "pushed_at": "2023-03-23T09:00:26Z",
          "releases_url": "https://api.github.com/repos/realmarv/conventions/releases{/id}",
          "size": 2395,
          "squash_merge_commit_message": "COMMIT_MESSAGES",
          "squash_merge_commit_title": "COMMIT_OR_PR_TITLE",
          "ssh_url": "git@github.com:realmarv/conventions.git",
          "stargazers_count": 0,
          "stargazers_url": "https://api.github.com/repos/realmarv/conventions/stargazers",
          "statuses_url": "https://api.github.com/repos/realmarv/conventions/statuses/{sha}",
          "subscribers_url": "https://api.github.com/repos/realmarv/conventions/subscribers",
          "subscription_url": "https://api.github.com/repos/realmarv/conventions/subscription",
          "svn_url": "https://github.com/realmarv/conventions",
          "tags_url": "https://api.github.com/repos/realmarv/conventions/tags",
          "teams_url": "https://api.github.com/repos/realmarv/conventions/teams",
          "topics": [],
          "trees_url": "https://api.github.com/repos/realmarv/conventions/git/trees{/sha}",
          "updated_at": "2022-10-13T08:53:39Z",
          "url": "https://api.github.com/repos/realmarv/conventions",
          "use_squash_pr_title_as_default": false,
          "visibility": "public",
          "watchers": 0,
          "watchers_count": 0,
          "web_commit_signoff_required": false
        },
        "sha": "0be2f2d608cdf893e146db1296dee750a2a54cfc",
        "user": {
          "avatar_url": "https://avatars.githubusercontent.com/u/50144546?v=4",
          "events_url": "https://api.github.com/users/realmarv/events{/privacy}",
          "followers_url": "https://api.github.com/users/realmarv/followers",
          "following_url": "https://api.github.com/users/realmarv/following{/other_user}",
          "gists_url": "https://api.github.com/users/realmarv/gists{/gist_id}",
          "gravatar_id": "",
          "html_url": "https://github.com/realmarv",
          "id": 50144546,
          "login": "realmarv",
          "node_id": "MDQ6VXNlcjUwMTQ0NTQ2",
          "organizations_url": "https://api.github.com/users/realmarv/orgs",
          "received_events_url": "https://api.github.com/users/realmarv/received_events",
          "repos_url": "https://api.github.com/users/realmarv/repos",
          "site_admin": false,
          "starred_url": "https://api.github.com/users/realmarv/starred{/owner}{/repo}",
          "subscriptions_url": "https://api.github.com/users/realmarv/subscriptions",
          "type": "User",
          "url": "https://api.github.com/users/realmarv"
        }
      },
      "body": null,
      "changed_files": 2,
      "closed_at": null,
      "comments": 0,
      "comments_url": "https://api.github.com/repos/realmarv/conventions/issues/4/comments",
      "commits": 2,
      "commits_url": "https://api.github.com/repos/realmarv/conventions/pulls/4/commits",
      "created_at": "2023-03-23T09:00:26Z",
      "deletions": 0,
      "diff_url": "https://github.com/realmarv/conventions/pull/4.diff",
      "draft": false,
      "head": {
        "label": "realmarv:fixGitPush1by1Check",
        "ref": "fixGitPush1by1Check",
        "repo": {
          "allow_auto_merge": false,
          "allow_forking": true,
          "allow_merge_commit": true,
          "allow_rebase_merge": true,
          "allow_squash_merge": true,
          "allow_update_branch": false,
          "archive_url": "https://api.github.com/repos/realmarv/conventions/{archive_format}{/ref}",
          "archived": false,
          "assignees_url": "https://api.github.com/repos/realmarv/conventions/assignees{/user}",
          "blobs_url": "https://api.github.com/repos/realmarv/conventions/git/blobs{/sha}",
          "branches_url": "https://api.github.com/repos/realmarv/conventions/branches{/branch}",
          "clone_url": "https://github.com/realmarv/conventions.git",
          "collaborators_url": "https://api.github.com/repos/realmarv/conventions/collaborators{/collaborator}",
          "comments_url": "https://api.github.com/repos/realmarv/conventions/comments{/number}",
          "commits_url": "https://api.github.com/repos/realmarv/conventions/commits{/sha}",
          "compare_url": "https://api.github.com/repos/realmarv/conventions/compare/{base}...{head}",
          "contents_url": "https://api.github.com/repos/realmarv/conventions/contents/{+path}",
          "contributors_url": "https://api.github.com/repos/realmarv/conventions/contributors",
          "created_at": "2022-10-12T08:48:57Z",
          "default_branch": "master",
          "delete_branch_on_merge": false,
          "deployments_url": "https://api.github.com/repos/realmarv/conventions/deployments",
          "description": null,
          "disabled": false,
          "downloads_url": "https://api.github.com/repos/realmarv/conventions/downloads",
          "events_url": "https://api.github.com/repos/realmarv/conventions/events",
          "fork": true,
          "forks": 0,
          "forks_count": 0,
          "forks_url": "https://api.github.com/repos/realmarv/conventions/forks",
          "full_name": "realmarv/conventions",
          "git_commits_url": "https://api.github.com/repos/realmarv/conventions/git/commits{/sha}",
          "git_refs_url": "https://api.github.com/repos/realmarv/conventions/git/refs{/sha}",
          "git_tags_url": "https://api.github.com/repos/realmarv/conventions/git/tags{/sha}",
          "git_url": "git://github.com/realmarv/conventions.git",
          "has_discussions": false,
          "has_downloads": true,
          "has_issues": false,
          "has_pages": false,
          "has_projects": true,
          "has_wiki": true,
          "homepage": null,
          "hooks_url": "https://api.github.com/repos/realmarv/conventions/hooks",
          "html_url": "https://github.com/realmarv/conventions",
          "id": 550128772,
          "is_template": false,
          "issue_comment_url": "https://api.github.com/repos/realmarv/conventions/issues/comments{/number}",
          "issue_events_url": "https://api.github.com/repos/realmarv/conventions/issues/events{/number}",
          "issues_url": "https://api.github.com/repos/realmarv/conventions/issues{/number}",
          "keys_url": "https://api.github.com/repos/realmarv/conventions/keys{/key_id}",
          "labels_url": "https://api.github.com/repos/realmarv/conventions/labels{/name}",
          "language": "TypeScript",
          "languages_url": "https://api.github.com/repos/realmarv/conventions/languages",
          "license": {
            "key": "mit",
            "name": "MIT License",
            "node_id": "MDc6TGljZW5zZTEz",
            "spdx_id": "MIT",
            "url": "https://api.github.com/licenses/mit"
          },
          "merge_commit_message": "PR_TITLE",
          "merge_commit_title": "MERGE_MESSAGE",
          "merges_url": "https://api.github.com/repos/realmarv/conventions/merges",
          "milestones_url": "https://api.github.com/repos/realmarv/conventions/milestones{/number}",
          "mirror_url": null,
          "name": "conventions",
          "node_id": "R_kgDOIMpMhA",
          "notifications_url": "https://api.github.com/repos/realmarv/conventions/notifications{?since,all,participating}",
          "open_issues": 4,
          "open_issues_count": 4,
          "owner": {
            "avatar_url": "https://avatars.githubusercontent.com/u/50144546?v=4",
            "events_url": "https://api.github.com/users/realmarv/events{/privacy}",
            "followers_url": "https://api.github.com/users/realmarv/followers",
            "following_url": "https://api.github.com/users/realmarv/following{/other_user}",
            "gists_url": "https://api.github.com/users/realmarv/gists{/gist_id}",
            "gravatar_id": "",
            "html_url": "https://github.com/realmarv",
            "id": 50144546,
            "login": "realmarv",
            "node_id": "MDQ6VXNlcjUwMTQ0NTQ2",
            "organizations_url": "https://api.github.com/users/realmarv/orgs",
            "received_events_url": "https://api.github.com/users/realmarv/received_events",
            "repos_url": "https://api.github.com/users/realmarv/repos",
            "site_admin": false,
            "starred_url": "https://api.github.com/users/realmarv/starred{/owner}{/repo}",
            "subscriptions_url": "https://api.github.com/users/realmarv/subscriptions",
            "type": "User",
            "url": "https://api.github.com/users/realmarv"
          },
          "private": false,
          "pulls_url": "https://api.github.com/repos/realmarv/conventions/pulls{/number}",
          "pushed_at": "2023-03-23T09:00:26Z",
          "releases_url": "https://api.github.com/repos/realmarv/conventions/releases{/id}",
          "size": 2395,
          "squash_merge_commit_message": "COMMIT_MESSAGES",
          "squash_merge_commit_title": "COMMIT_OR_PR_TITLE",
          "ssh_url": "git@github.com:realmarv/conventions.git",
          "stargazers_count": 0,
          "stargazers_url": "https://api.github.com/repos/realmarv/conventions/stargazers",
          "statuses_url": "https://api.github.com/repos/realmarv/conventions/statuses/{sha}",
          "subscribers_url": "https://api.github.com/repos/realmarv/conventions/subscribers",
          "subscription_url": "https://api.github.com/repos/realmarv/conventions/subscription",
          "svn_url": "https://github.com/realmarv/conventions",
          "tags_url": "https://api.github.com/repos/realmarv/conventions/tags",
          "teams_url": "https://api.github.com/repos/realmarv/conventions/teams",
          "topics": [],
          "trees_url": "https://api.github.com/repos/realmarv/conventions/git/trees{/sha}",
          "updated_at": "2022-10-13T08:53:39Z",
          "url": "https://api.github.com/repos/realmarv/conventions",
          "use_squash_pr_title_as_default": false,
          "visibility": "public",
          "watchers": 0,
          "watchers_count": 0,
          "web_commit_signoff_required": false
        },
        "sha": "c5eb50ea6c5a72d61ab85d96ffc66cc4741d3133",
        "user": {
          "avatar_url": "https://avatars.githubusercontent.com/u/50144546?v=4",
          "events_url": "https://api.github.com/users/realmarv/events{/privacy}",
          "followers_url": "https://api.github.com/users/realmarv/followers",
          "following_url": "https://api.github.com/users/realmarv/following{/other_user}",
          "gists_url": "https://api.github.com/users/realmarv/gists{/gist_id}",
          "gravatar_id": "",
          "html_url": "https://github.com/realmarv",
          "id": 50144546,
          "login": "realmarv",
          "node_id": "MDQ6VXNlcjUwMTQ0NTQ2",
          "organizations_url": "https://api.github.com/users/realmarv/orgs",
          "received_events_url": "https://api.github.com/users/realmarv/received_events",
          "repos_url": "https://api.github.com/users/realmarv/repos",
          "site_admin": false,
          "starred_url": "https://api.github.com/users/realmarv/starred{/owner}{/repo}",
          "subscriptions_url": "https://api.github.com/users/realmarv/subscriptions",
          "type": "User",
          "url": "https://api.github.com/users/realmarv"
        }
      },
      "html_url": "https://github.com/realmarv/conventions/pull/4",
      "id": 1287033164,
      "issue_url": "https://api.github.com/repos/realmarv/conventions/issues/4",
      "labels": [],
      "locked": false,
      "maintainer_can_modify": false,
      "merge_commit_sha": null,
      "mergeable": null,
      "mergeable_state": "unknown",
      "merged": false,
      "merged_at": null,
      "merged_by": null,
      "milestone": null,
      "node_id": "PR_kwDOIMpMhM5MtpFM",
      "number": 4,
      "patch_url": "https://github.com/realmarv/conventions/pull/4.patch",
      "rebaseable": null,
      "requested_reviewers": [],
      "requested_teams": [],
      "review_comment_url": "https://api.github.com/repos/realmarv/conventions/pulls/comments{/number}",
      "review_comments": 0,
      "review_comments_url": "https://api.github.com/repos/realmarv/conventions/pulls/4/comments",
      "state": "open",
      "statuses_url": "https://api.github.com/repos/realmarv/conventions/statuses/c5eb50ea6c5a72d61ab85d96ffc66cc4741d3133",
      "title": "Fix git push1by1 check",
      "updated_at": "2023-03-23T09:00:26Z",
      "url": "https://api.github.com/repos/realmarv/conventions/pulls/4",
      "user": {
        "avatar_url": "https://avatars.githubusercontent.com/u/50144546?v=4",
        "events_url": "https://api.github.com/users/realmarv/events{/privacy}",
        "followers_url": "https://api.github.com/users/realmarv/followers",
        "following_url": "https://api.github.com/users/realmarv/following{/other_user}",
        "gists_url": "https://api.github.com/users/realmarv/gists{/gist_id}",
        "gravatar_id": "",
        "html_url": "https://github.com/realmarv",
        "id": 50144546,
        "login": "realmarv",
        "node_id": "MDQ6VXNlcjUwMTQ0NTQ2",
        "organizations_url": "https://api.github.com/users/realmarv/orgs",
        "received_events_url": "https://api.github.com/users/realmarv/received_events",
        "repos_url": "https://api.github.com/users/realmarv/repos",
        "site_admin": false,
        "starred_url": "https://api.github.com/users/realmarv/starred{/owner}{/repo}",
        "subscriptions_url": "https://api.github.com/users/realmarv/subscriptions",
        "type": "User",
        "url": "https://api.github.com/users/realmarv"
      }
    },
    "repository": {
      "allow_forking": true,
      "archive_url": "https://api.github.com/repos/realmarv/conventions/{archive_format}{/ref}",
      "archived": false,
      "assignees_url": "https://api.github.com/repos/realmarv/conventions/assignees{/user}",
      "blobs_url": "https://api.github.com/repos/realmarv/conventions/git/blobs{/sha}",
      "branches_url": "https://api.github.com/repos/realmarv/conventions/branches{/branch}",
      "clone_url": "https://github.com/realmarv/conventions.git",
      "collaborators_url": "https://api.github.com/repos/realmarv/conventions/collaborators{/collaborator}",
      "comments_url": "https://api.github.com/repos/realmarv/conventions/comments{/number}",
      "commits_url": "https://api.github.com/repos/realmarv/conventions/commits{/sha}",
      "compare_url": "https://api.github.com/repos/realmarv/conventions/compare/{base}...{head}",
      "contents_url": "https://api.github.com/repos/realmarv/conventions/contents/{+path}",
      "contributors_url": "https://api.github.com/repos/realmarv/conventions/contributors",
      "created_at": "2022-10-12T08:48:57Z",
      "default_branch": "master",
      "deployments_url": "https://api.github.com/repos/realmarv/conventions/deployments",
      "description": null,
      "disabled": false,
      "downloads_url": "https://api.github.com/repos/realmarv/conventions/downloads",
      "events_url": "https://api.github.com/repos/realmarv/conventions/events",
      "fork": true,
      "forks": 0,
      "forks_count": 0,
      "forks_url": "https://api.github.com/repos/realmarv/conventions/forks",
      "full_name": "realmarv/conventions",
      "git_commits_url": "https://api.github.com/repos/realmarv/conventions/git/commits{/sha}",
      "git_refs_url": "https://api.github.com/repos/realmarv/conventions/git/refs{/sha}",
      "git_tags_url": "https://api.github.com/repos/realmarv/conventions/git/tags{/sha}",
      "git_url": "git://github.com/realmarv/conventions.git",
      "has_discussions": false,
      "has_downloads": true,
      "has_issues": false,
      "has_pages": false,
      "has_projects": true,
      "has_wiki": true,
      "homepage": null,
      "hooks_url": "https://api.github.com/repos/realmarv/conventions/hooks",
      "html_url": "https://github.com/realmarv/conventions",
      "id": 550128772,
      "is_template": false,
      "issue_comment_url": "https://api.github.com/repos/realmarv/conventions/issues/comments{/number}",
      "issue_events_url": "https://api.github.com/repos/realmarv/conventions/issues/events{/number}",
      "issues_url": "https://api.github.com/repos/realmarv/conventions/issues{/number}",
      "keys_url": "https://api.github.com/repos/realmarv/conventions/keys{/key_id}",
      "labels_url": "https://api.github.com/repos/realmarv/conventions/labels{/name}",
      "language": "TypeScript",
      "languages_url": "https://api.github.com/repos/realmarv/conventions/languages",
      "license": {
        "key": "mit",
        "name": "MIT License",
        "node_id": "MDc6TGljZW5zZTEz",
        "spdx_id": "MIT",
        "url": "https://api.github.com/licenses/mit"
      },
      "merges_url": "https://api.github.com/repos/realmarv/conventions/merges",
      "milestones_url": "https://api.github.com/repos/realmarv/conventions/milestones{/number}",
      "mirror_url": null,
      "name": "conventions",
      "node_id": "R_kgDOIMpMhA",
      "notifications_url": "https://api.github.com/repos/realmarv/conventions/notifications{?since,all,participating}",
      "open_issues": 4,
      "open_issues_count": 4,
      "owner": {
        "avatar_url": "https://avatars.githubusercontent.com/u/50144546?v=4",
        "events_url": "https://api.github.com/users/realmarv/events{/privacy}",
        "followers_url": "https://api.github.com/users/realmarv/followers",
        "following_url": "https://api.github.com/users/realmarv/following{/other_user}",
        "gists_url": "https://api.github.com/users/realmarv/gists{/gist_id}",
        "gravatar_id": "",
        "html_url": "https://github.com/realmarv",
        "id": 50144546,
        "login": "realmarv",
        "node_id": "MDQ6VXNlcjUwMTQ0NTQ2",
        "organizations_url": "https://api.github.com/users/realmarv/orgs",
        "received_events_url": "https://api.github.com/users/realmarv/received_events",
        "repos_url": "https://api.github.com/users/realmarv/repos",
        "site_admin": false,
        "starred_url": "https://api.github.com/users/realmarv/starred{/owner}{/repo}",
        "subscriptions_url": "https://api.github.com/users/realmarv/subscriptions",
        "type": "User",
        "url": "https://api.github.com/users/realmarv"
      },
      "private": false,
      "pulls_url": "https://api.github.com/repos/realmarv/conventions/pulls{/number}",
      "pushed_at": "2023-03-23T09:00:26Z",
      "releases_url": "https://api.github.com/repos/realmarv/conventions/releases{/id}",
      "size": 2395,
      "ssh_url": "git@github.com:realmarv/conventions.git",
      "stargazers_count": 0,
      "stargazers_url": "https://api.github.com/repos/realmarv/conventions/stargazers",
      "statuses_url": "https://api.github.com/repos/realmarv/conventions/statuses/{sha}",
      "subscribers_url": "https://api.github.com/repos/realmarv/conventions/subscribers",
      "subscription_url": "https://api.github.com/repos/realmarv/conventions/subscription",
      "svn_url": "https://github.com/realmarv/conventions",
      "tags_url": "https://api.github.com/repos/realmarv/conventions/tags",
      "teams_url": "https://api.github.com/repos/realmarv/conventions/teams",
      "topics": [],
      "trees_url": "https://api.github.com/repos/realmarv/conventions/git/trees{/sha}",
      "updated_at": "2022-10-13T08:53:39Z",
      "url": "https://api.github.com/repos/realmarv/conventions",
      "visibility": "public",
      "watchers": 0,
      "watchers_count": 0,
      "web_commit_signoff_required": false
    },
    "sender": {
      "avatar_url": "https://avatars.githubusercontent.com/u/50144546?v=4",
      "events_url": "https://api.github.com/users/realmarv/events{/privacy}",
      "followers_url": "https://api.github.com/users/realmarv/followers",
      "following_url": "https://api.github.com/users/realmarv/following{/other_user}",
      "gists_url": "https://api.github.com/users/realmarv/gists{/gist_id}",
      "gravatar_id": "",
      "html_url": "https://github.com/realmarv",
      "id": 50144546,
      "login": "realmarv",
      "node_id": "MDQ6VXNlcjUwMTQ0NTQ2",
      "organizations_url": "https://api.github.com/users/realmarv/orgs",
      "received_events_url": "https://api.github.com/users/realmarv/received_events",
      "repos_url": "https://api.github.com/users/realmarv/repos",
      "site_admin": false,
      "starred_url": "https://api.github.com/users/realmarv/starred{/owner}{/repo}",
      "subscriptions_url": "https://api.github.com/users/realmarv/subscriptions",
      "type": "User",
      "url": "https://api.github.com/users/realmarv"
    }
  }
""">

let jsonString = File.ReadAllText githubEventPath
let parsedJsonObj = githubEventType.Parse jsonString

let gitForkUser = parsedJsonObj.PullRequest.Head.User.Login
let gitForkRepo = parsedJsonObj.PullRequest.Head.Repo.Name
let gitRepo = $"{gitForkUser}/{gitForkRepo}"

let currentBranch =
    Process
        .Execute(
            {
                Command = "git"
                Arguments = "rev-parse --abbrev-ref HEAD"
            },
            Process.Echo.Off
        )
        .UnwrapDefault()
        .Trim()

let prCommits =
    Process
        .Execute(
            {
                Command = "git"
                Arguments =
                    sprintf "rev-list %s~..%s" currentBranch currentBranch
            },
            Process.Echo.Off
        )
        .UnwrapDefault()
        .Trim()
        .Split "\n"
    |> Seq.tail

let notUsingGitPush1by1 =
    prCommits
    |> Seq.map(fun commit ->
        use client = new HttpClient()
        client.DefaultRequestHeaders.Accept.Clear()

        client.DefaultRequestHeaders.Accept.Add(
            MediaTypeWithQualityHeaderValue "application/vnd.github+json"
        )

        client.DefaultRequestHeaders.Add("User-Agent", ".NET App")
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28")

        let url =
            sprintf
                "https://api.github.com/repos/%s/commits/%s/check-suites"
                gitRepo
                commit

        let json = (client.GetStringAsync url).Result

        json.Contains "\"check_suites\":[]"
    )
    |> Seq.contains true

if notUsingGitPush1by1 then
    let errMsg =
        sprintf
            "Please push the commits one by one; using this script is recommended:%s%s"
            Environment.NewLine
            "https://github.com/nblockchain/conventions/blob/master/scripts/gitPush1by1.fsx"

    Console.Error.WriteLine errMsg
    Environment.Exit 1
