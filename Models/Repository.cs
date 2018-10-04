using System;
using Newtonsoft.Json;

namespace repos_stats.Models
{

    public class Repository
    {
        public Repository() { }

        [JsonProperty("id")]
        public long Id { get; protected set; }

        /// <summary>
        /// GraphQL Node Id
        /// </summary>
        [JsonProperty("node_id")]
        public string NodeId { get; protected set; }

        [JsonProperty("owner")]
        public User Owner { get; protected set; }

        [JsonProperty("name")]
        public string Name { get; protected set; }

        [JsonProperty("full_name")]
        public string FullName { get; protected set; }

        [JsonProperty("forks_count")]
        public int ForksCount { get; protected set; }

        [JsonProperty("stargazers_count")]
        public int StargazersCount { get; protected set; }
        
        [JsonProperty("default_branch")]
        public string DefaultBranch { get; protected set; }

        [JsonProperty("open_issues_count")]
        public int OpenIssuesCount { get; protected set; }

        [JsonProperty("permissions")]
        public RepositoryPermissions Permissions { get; protected set; }

        [JsonProperty("has_issues")]
        public bool HasIssues { get; protected set; }

        [JsonProperty("has_wiki")]
        public bool HasWiki { get; protected set; }

        [JsonProperty("has_downloads")]
        public bool HasDownloads { get; protected set; }

        [JsonProperty("has_pages")]
        public bool HasPages { get; protected set; }

        [JsonProperty("subscribers_count")]
        public int SubscribersCount { get; protected set; }
    }
}