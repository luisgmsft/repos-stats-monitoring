using Newtonsoft.Json;

namespace repos_stats.Models
{
    public class RepositoryPermissions
    {
        public RepositoryPermissions() { }

        /// <summary>
        /// Whether the current user has administrative permissions
        /// </summary>
        [JsonProperty("admin")]
        public bool Admin { get; protected set; }

        /// <summary>
        /// Whether the current user has push permissions
        /// </summary>
        [JsonProperty("push")]
        public bool Push { get; protected set; }

        /// <summary>
        /// Whether the current user has pull permissions
        /// </summary>
        [JsonProperty("pull")]
        public bool Pull { get; protected set; }
    }
}