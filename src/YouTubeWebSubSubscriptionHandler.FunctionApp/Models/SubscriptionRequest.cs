using Newtonsoft.Json;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This represents the request entity for subscriptions.
    /// </summary>
    public class SubscriptionRequest
    {
        /// <summary>
        /// Gets or sets the topic URI.
        /// </summary>
        [JsonProperty("topicUri")]
        public virtual string TopicUri { get; set; } = "https://www.youtube.com/xml/feeds/videos.xml?channel_id=<CHANNEL_ID>";

        /// <summary>
        /// Gets or sets the subscription mode.
        /// </summary>
        [JsonProperty("mode")]
        public virtual SubscriptionMode Mode { get; set; } = SubscriptionMode.Subscribe;
    }
}
