using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Constants;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Extensions;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Services
{
    /// <summary>
    /// This represents the service entity for the subscription requests.
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionService" /> class.
        /// </summary>
        /// <param name="settings"><see cref="AppSettings" /> instance.</param>
        /// <param name="httpClient"><see cref="HttpClient" /> instance.</param>
        public SubscriptionService(AppSettings settings, HttpClient httpClient)
        {
            this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc />
        public async Task<SubscriptionResponse> ProcessSubscription(SubscriptionRequest req)
        {
            var requestUri = new Uri(this._settings.WebSub.SubscriptionUri);
            var values = new Dictionary<string, string>()
            {
                { SubscriptionKeys.HubCallback, $"{this._settings.WebSub.CallbackUri}?code={this._settings.WebSub.CallbackKey}" },
                { SubscriptionKeys.HubTopic, req.TopicUri },
                { SubscriptionKeys.HubVerify, SubscriptionVerificationType.Asynchronous.ToValueString() },
                { SubscriptionKeys.HubMode, req.Mode.ToValueString() },
                { SubscriptionKeys.HubVerifyToken, string.Empty },
                { SubscriptionKeys.HubSecret, string.Empty },
                { SubscriptionKeys.HubLeaseSeconds, string.Empty },
            };

            var result = default(SubscriptionResponse);
            using (var content = new FormUrlEncodedContent(values))
            using (var response = await this._httpClient.PostAsync(requestUri, content).ConfigureAwait(false))
            {
                var headers = response.Headers.ToDictionary(p => p.Key, p => string.Join("|", p.Value));

                result = new SubscriptionResponse() { StatusCode = response.StatusCode, Headers = headers };
            }

            return result;
        }
    }
}
