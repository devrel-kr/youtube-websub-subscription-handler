using System;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Constants;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This represents the query entity to verify subscription requests.
    /// </summary>
    public class SubscriptionVerificationQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionVerificationQuery" /> class.
        /// </summary>
        public SubscriptionVerificationQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionVerificationQuery" /> class.
        /// </summary>
        /// <param name="query"><see cref="IQueryCollection" /> instance.</param>
        public SubscriptionVerificationQuery(IQueryCollection query)
        {
            this.HubTopic = GetTopicUri(query[SubscriptionKeys.HubTopic]);
            this.HubChallenge = query[SubscriptionKeys.HubChallenge];
            this.HubMode = GetSubscriptionMode(query[SubscriptionKeys.HubMode]);
            this.HubLeaseSeconds = GetLeaseSeconds(query[SubscriptionKeys.HubLeaseSeconds]);
        }

        /// <summary>
        /// Gets or sets the topic URI.
        /// </summary>
        [JsonProperty(SubscriptionKeys.HubTopic)]
        public Uri HubTopic { get; set; }

        /// <summary>
        /// Gets or sets the challenge value.
        /// </summary>
        [JsonProperty(SubscriptionKeys.HubChallenge)]
        public string HubChallenge { get; set; }

        /// <summary>
        /// Gets or sets the subscription mode value.
        /// </summary>
        [JsonProperty(SubscriptionKeys.HubMode)]
        public SubscriptionMode HubMode { get; set; }

        /// <summary>
        /// Gets or sets the expiration period in seconds.
        /// </summary>
        [JsonProperty(SubscriptionKeys.HubLeaseSeconds)]
        public long HubLeaseSeconds { get; set; }

        private static Uri GetTopicUri(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            var topic = new Uri(value);

            return topic;
        }

        private static SubscriptionMode GetSubscriptionMode(string value)
        {
            var mode = Enum.Parse<SubscriptionMode>(value, ignoreCase: true);

            return mode;
        }

        private static long GetLeaseSeconds(string value)
        {
            var secs = string.IsNullOrWhiteSpace(value) ? long.MinValue : Convert.ToInt64(value);

            return secs;
        }
    }
}
