using System;

using Newtonsoft.Json;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This represents the request entity for subscriptions.
    /// </summary>
    public class FetchRequest
    {
        /// <summary>
        /// Gets or sets the Event ID.
        /// </summary>
        [JsonProperty("id")]
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the CloudEvents spec version. This MUST be "1.0".
        /// </summary>
        [JsonProperty("specversion")]
        public virtual string SpecVersion { get; set; }

        /// <summary>
        /// Gets or sets the event source. This MUST be in the format of "https://www.youtube.com/xml/feeds/videos.xml?channel_id=[CHANNEL_ID]".
        /// </summary>
        [JsonProperty("source")]
        public virtual string Source { get; set; }

        /// <summary>
        /// Gets or sets the event type. This MUST be "com.youtube.video.published".
        /// </summary>
        [JsonProperty("type")]
        public virtual string Type { get; set; }

        /// <summary>
        /// Gets or sets the time when the event was published.
        /// </summary>
        [JsonProperty("time")]
        public virtual DateTimeOffset Time { get; set; }

        /// <summary>
        /// Gets or sets the event data content type. This MUST be "application/cloudevents+json".
        /// </summary>
        [JsonProperty("datacontenttype")]
        public virtual string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the event data. This MUST be XML string.
        /// </summary>
        [JsonProperty("data")]
        public virtual string Data { get; set; }

        /// <summary>
        /// Gets or sets the trace parent value.
        /// </summary>
        [JsonProperty("traceparent")]
        public virtual string TraceParent { get; set; }
    }
}
