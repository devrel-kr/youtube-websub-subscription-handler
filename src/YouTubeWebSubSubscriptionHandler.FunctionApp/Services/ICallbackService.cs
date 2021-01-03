using System.Collections.Generic;
using System.Threading.Tasks;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models;

using Microsoft.AspNetCore.Http;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Services
{
    /// <summary>
    /// This provides interfaces to <see cref="CallbackService" /> class.
    /// </summary>
    public interface ICallbackService
    {
        /// <summary>
        /// Processes the subscription request verification.
        /// </summary>
        /// <param name="method">HTTP request method.</param>
        /// <param name="query">Request querystring collection.</param>
        /// <returns>Returns the <see cref="VerificationResponse" /> instance.</returns>
        Task<VerificationResponse> ProcessVerificationAsync(string method, IQueryCollection query);

        /// <summary>
        /// Processes the event handling.
        /// </summary>
        /// <param name="method">HTTP request method.</param>
        /// <param name="payload">Event payload.</param>
        /// <param name="links">WebSub links.</param>
        /// <returns>Returns the <see cref="CallbackResponse" /> instance.</returns>
        Task<CallbackResponse> ProcessEventAsync(string method, string payload, Dictionary<string, string> links);
    }
}
