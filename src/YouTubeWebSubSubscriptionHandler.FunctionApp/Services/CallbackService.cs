using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Azure.Messaging.EventGrid;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Constants;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Extensions;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models;

using Microsoft.AspNetCore.Http;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Services
{
    /// <summary>
    /// This represents the service entity for the subscription callbacks.
    /// </summary>
    public class CallbackService : ICallbackService
    {
        private readonly AppSettings _settings;
        private readonly EventGridPublisherClient _publisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackService" /> class.
        /// </summary>
        /// <param name="settings"><see cref="AppSettings" /> instance.</param>
        /// <param name="publisher"><see cref="EventGridPublisherClient" /> instance.</param>
        public CallbackService(AppSettings settings, EventGridPublisherClient publisher)
        {
            this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        /// <inheritdoc />
        public async Task<VerificationResponse> ProcessVerificationAsync(string method, IQueryCollection query)
        {
            if (!HttpMethods.IsGet(method))
            {
                return null;
            }

            var queries = query.Any() ? new SubscriptionVerificationQuery(query) : null;

            var result = default(VerificationResponse);
            if (!IsRequestValid(method, queries))
            {
                result = new VerificationResponse() { StatusCode = HttpStatusCode.BadRequest, Mode = queries.HubMode };

                return await Task.FromResult(result).ConfigureAwait(false);
            }

            result = new VerificationResponse() { StatusCode = HttpStatusCode.OK, Mode = queries.HubMode, Challenge = queries.HubChallenge };

            return await Task.FromResult(result).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallbackResponse> ProcessEventAsync(string method, string payload, Dictionary<string, string> links)
        {
            if (!HttpMethods.IsPost(method))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            var source = links[CallbackKeys.RelSelf];
            var type = GetEventType(payload).ToValueString();
            var dataContentType = CallbackKeys.DataContentType;

            var @event = new CloudEvent(source, type, payload, dataContentType);
            var events = new List<CloudEvent>() { @event };

            var result = default(CallbackResponse);
            using (var response = await this._publisher.SendEventsAsync(events).ConfigureAwait(false))
            {
                var headers = response.Headers.ToDictionary(p => p.Name, p => p.Value);

                result = new CallbackResponse() { StatusCode = (HttpStatusCode)response.Status, Headers = headers };
            }

            return result;
        }

        private static bool IsRequestValid(string method, SubscriptionVerificationQuery queries)
        {
            return HttpMethods.IsGet(method) && queries != null && queries.HubMode != SubscriptionMode.Undefined && !string.IsNullOrWhiteSpace(queries.HubChallenge);
        }

        private static YouTubeVideoEventType GetEventType(string payload)
        {
            var isEntryDeleted = payload.IndexOf(CallbackKeys.DeletedEntry) >= 0;
            if (isEntryDeleted)
            {
                return YouTubeVideoEventType.Unpublished;
            }

            return YouTubeVideoEventType.Published;
        }
    }
}
