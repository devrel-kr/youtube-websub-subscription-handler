using System;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This represents the entity for YouTube video item details.
    /// </summary>
    public class VideoItemDetails
    {
        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        public virtual string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the video ID.
        /// </summary>
        public virtual string VideoId { get; set; }

        /// <summary>
        /// Gets or sets the video link URL.
        /// </summary>
        public virtual Uri Link { get; set; }

        /// <summary>
        /// Gets or sets the date of video published.
        /// </summary>
        public virtual DateTimeOffset DatePublished { get; set; }

        /// <summary>
        /// Gets or sets the date of video updated.
        /// </summary>
        public virtual DateTimeOffset DateUpdated { get; set; }
    }
}
