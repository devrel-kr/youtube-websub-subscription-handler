using System.Threading.Tasks;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Services
{
    /// <summary>
    /// This provides interfaces to <see cref="FetchService" /> class.
    /// </summary>
    public interface IFetchService
    {
        /// <summary>
        /// Extracts the video details from the XML feed.
        /// </summary>
        /// <param name="req"><see cref="FetchRequest" /> instance.</param>
        /// <returns>Returns the <see cref="VideoItemDetails" /> instance.</returns>
        Task<VideoItemDetails> ExtractVideoDetailsAsync(FetchRequest req);

        /// <summary>
        /// Fetches the video details from YouTube API.
        /// </summary>
        /// <param name="item"><see cref="VideoItemDetails" /> instance.</param>
        /// <returns>Returns the <see cref="FetchResponse" /> instance.</returns>
        Task<FetchResponse> FetchVideoDetailsAsync(VideoItemDetails item);
    }
}
