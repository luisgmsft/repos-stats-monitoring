using System;
using Newtonsoft.Json;

namespace repos_stats.Models
{
    public class RepositoryTrafficView
    {
        public RepositoryTrafficView() { }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; protected set; }
        
        [JsonProperty("count")]
        public int Count { get; protected set; }

        [JsonProperty("uniques")]
        public int Uniques { get; protected set; }
    }
}