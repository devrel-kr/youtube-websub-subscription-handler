using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Constants;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models;

using Google.Apis.Util;
using Google.Apis.YouTube.v3;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Services
{
    /// <summary>
    /// This represents the service entity to fetch YouTube video details.
    /// </summary>
    /// <remarks>
    /// <para>This class implementation has the following external references:</para>
    /// <list>
    /// <item>Docs: https://developers.google.com/youtube/v3/docs/videos</item>
    /// <item>Developer Console: https://console.developers.google.com/apis/dashboard</item>
    /// <item>NuGet Package: https://www.nuget.org/packages/Google.Apis.YouTube.v3</item>
    /// <item>GitHub Repository: https://github.com/googleapis/google-api-dotnet-client</item>
    /// </list>
    /// </remarks>
    public class FetchService : IFetchService
    {
        private readonly AppSettings _settings;
        private readonly VideosResource _resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchService" /> class.
        /// </summary>
        /// <param name="settings"><see cref="AppSettings" /> instance.</param>
        /// <param name="resource"><see cref="VideosResource" /> instance.</param>
        public FetchService(AppSettings settings, VideosResource resource)
        {
            this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this._resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <inheritdoc />
        public async Task<VideoItemDetails> ExtractVideoDetailsAsync(FetchRequest req)
        {
            var feed = await Task.Factory.StartNew(() => {
                using (var xml = new StringReader(req.Data))
                using (var reader = XmlReader.Create(xml))
                {
                    return SyndicationFeed.Load(reader);
                }
            }).ConfigureAwait(false);

            var item = feed.Items.First();
            var videoId = item.ElementExtensions.FirstOrDefault(p => p.OuterName == FetchKeys.VideoId).GetObject<string>();
            var channelId = item.ElementExtensions.FirstOrDefault(p => p.OuterName == FetchKeys.ChannelId).GetObject<string>();
            var link = item.Links.First().Uri;
            var published = item.PublishDate;
            var updated = item.LastUpdatedTime;

            var details = new VideoItemDetails()
            {
                ChannelId = channelId,
                VideoId = videoId,
                Link = link,
                DatePublished = published,
                DateUpdated = updated,
            };

            return details;
        }

        /// <inheritdoc />
        public async Task<FetchResponse> FetchVideoDetailsAsync(VideoItemDetails item)
        {
            var part = new Repeatable<string>(this._settings.YouTube.FetchParts);
            var list = this._resource.List(part);
            list.Id = item.VideoId;

            var result = await list.ExecuteAsync().ConfigureAwait(false);

            var snippet = result.Items[0].Snippet;
            var title = snippet.Title;
            var description = snippet.Description;
            var thumbnail = snippet.Thumbnails.Maxres.Url;

            var response = new FetchResponse()
            {
                ChannelId = item.ChannelId,
                VideoId = item.VideoId,
                Title = title,
                Description = description,
                Link = item.Link,
                ThumbnailLink = new Uri(thumbnail),
                DatePublished = item.DatePublished,
                DateUpdated = item.DateUpdated
            };

            return response;
        }
    }
}
