using System.Collections.Generic;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This represents the response entity for subscriptions.
    /// </summary>
    public class SubscriptionResponse : Response
    {
        /// <summary>
        /// Gets or sets the list of headers from the subscription response.
        /// </summary>
        /// <typeparam name="string">Header key.</typeparam>
        /// <typeparam name="string">Heaver value.</typeparam>
        public virtual Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}
