using System.Net;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This represents the response entity. This MUST be inherited.
    /// </summary>
    public abstract class Response
    {
        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode" /> value.
        /// </summary>
        public virtual HttpStatusCode StatusCode { get; set; }
    }
}
