using System;

using Newtonsoft.Json;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This represents the response entity for YouTube video fetch details.
    /// </summary>
    public class FetchResponse
    {
        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        [JsonProperty("channelId")]
        public virtual string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the video ID.
        /// </summary>
        [JsonProperty("videoId")]
        public virtual string VideoId { get; set; }

        /// <summary>
        /// Gets or sets the video title.
        /// </summary>
        [JsonProperty("title")]
        public virtual string Title { get; set; }

        /// <summary>
        /// Gets or sets the video description.
        /// </summary>
        [JsonProperty("description")]
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the video link URL.
        /// </summary>
        [JsonProperty("link")]
        public virtual Uri Link { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail link URL.
        /// </summary>
        [JsonProperty("thumbnailLink")]
        public virtual Uri ThumbnailLink { get; set; }

        /// <summary>
        /// Gets or sets the date of video published.
        /// </summary>
        [JsonProperty("datePublished")]
        public virtual DateTimeOffset DatePublished { get; set; }

        /// <summary>
        /// Gets or sets the date of video updated.
        /// </summary>
        [JsonProperty("dateUpdated")]
        public virtual DateTimeOffset DateUpdated { get; set; }
    }
}
