using Newtonsoft.Json;
using System;

namespace repos_stats.Models
{
    public class User
    {
        public User() { }

        /// <summary>
        /// The account's system-wide unique Id.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; protected set; }

        /// <summary>
        /// GraphQL Node Id
        /// </summary>
        [JsonProperty("node_id")]
        public string NodeId { get; protected set; }

        /// <summary>
        /// The account's login.
        /// </summary>
        [JsonProperty("login")]
        public string Login { get; protected set; }
        /// <summary>
        /// Whether or not the user is an administrator of the site
        /// </summary>
        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; protected set; }
    }
}