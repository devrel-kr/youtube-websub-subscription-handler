using System.Threading.Tasks;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Services
{
    /// <summary>
    /// This provides interfaces to <see cref="SubscriptionService" /> class.
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Processes the subscription request.
        /// </summary>
        /// <param name="req"><see cref="SubscriptionRequest" /> instance.</param>
        /// <returns>Returns the <see cref="SubscriptionResponse" /> instance.</returns>
        Task<SubscriptionResponse> ProcessSubscription(SubscriptionRequest req);
    }
}
