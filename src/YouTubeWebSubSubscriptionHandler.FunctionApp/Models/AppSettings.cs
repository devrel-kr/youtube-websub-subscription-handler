using System;

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
}
