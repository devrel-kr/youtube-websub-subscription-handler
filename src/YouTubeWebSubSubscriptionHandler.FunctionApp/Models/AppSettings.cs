using System;
using System.Collections.Generic;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This represents the app settings entity.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets the <see cref="WebSubSettings"/> instance.
        /// </summary>
        public virtual WebSubSettings WebSub { get; } = new WebSubSettings();

        /// <summary>
        /// Gets the <see cref="EventGridSettings"/> instance.
        /// </summary>
        public virtual EventGridSettings EventGrid { get; } = new EventGridSettings();

        /// <summary>
        /// Gets the <see cref="YouTubeSettings"/> instance.
        /// </summary>
        public virtual YouTubeSettings YouTube { get; } = new YouTubeSettings();
    }

    /// <summary>
    /// This represents the app settings entity for WebSub.
    /// </summary>
    public class WebSubSettings
    {
        /// <summary>
        /// Gets the URI for subscription.
        /// </summary>
        public virtual string SubscriptionUri { get; } = Environment.GetEnvironmentVariable("WebSub__SubscriptionUri");

        /// <summary>
        /// Gets the URI for callback.
        /// </summary>
        public virtual string CallbackUri { get; } = Environment.GetEnvironmentVariable("WebSub__CallbackUri");

        /// <summary>
        /// Gets the API key for callback.
        /// </summary>
        public virtual string CallbackKey { get; } = Environment.GetEnvironmentVariable("WebSub__CallbackKey");
    }

    /// <summary>
    /// This represents the app settings entity for EventGrid.
    /// </summary>
    public class EventGridSettings
    {
        /// <summary>
        /// Gets the <see cref="EventGridTopicSettings"/> instance.
        /// </summary>
        public virtual EventGridTopicSettings Topic { get; } = new EventGridTopicSettings();
    }

    /// <summary>
    /// This represents the app settings entity for EventGrid Topic.
    /// </summary>
    public class EventGridTopicSettings
    {
        /// <summary>
        /// Gets the endpoint URI for EventGrid Topic.
        /// </summary>
        public virtual string Endpoint { get; } = Environment.GetEnvironmentVariable("EventGrid__Topic__Endpoint");

        /// <summary>
        /// Gets the access key to EventGrid Topic.
        /// </summary>
        public virtual string AccessKey { get; } = Environment.GetEnvironmentVariable("EventGrid__Topic__AccessKey");
    }

    /// <summary>
    /// This represents the app settings entity for YouTube API.
    /// </summary>
    public class YouTubeSettings
    {
        /// <summary>
        /// Gets the API key.
        /// </summary>
        public virtual string ApiKey { get; } = Environment.GetEnvironmentVariable("YouTube__ApiKey");

        /// <summary>
        /// Gets the list of parts to fetch.
        /// </summary>
        public virtual IEnumerable<string> FetchParts { get; } = Environment.GetEnvironmentVariable("YouTube__FetchParts").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
    }
}
