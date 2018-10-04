using Newtonsoft.Json;
using System;

namespace repos_stats.Models
{
    public class RepositoryTrafficClone
    {
        public RepositoryTrafficClone() { }


        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; protected set; }

        [JsonProperty("count")]
        public int Count { get; protected set; }

        [JsonProperty("uniques")]
        public int Uniques { get; protected set; }
    }
}