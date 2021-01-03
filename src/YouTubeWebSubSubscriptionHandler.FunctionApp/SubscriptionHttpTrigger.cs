using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Constants;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp
{
    /// <summary>
    /// This represents the HTTP trigger entity to handle subscription requests.
    /// </summary>
    public class SubscriptionHttpTrigger
    {
        private readonly ISubscriptionService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionHttpTrigger" /> class.
        /// </summary>
        /// <param name="service"><see cref="ISubscriptionService" /> instance.</param>
        public SubscriptionHttpTrigger(ISubscriptionService service)
        {
            this._service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Invokes the subscription request handler.
        /// </summary>
        /// <param name="req"><see cref="HttpRequest" /> instance.</param>
        /// <param name="context"><see cref="ExecutionContext" /> instance.</param>
        /// <param name="log"><see cref="ILogger" /> instance.</param>
        /// <returns>Returns the <see cref="IActionResult" /> instance.</returns>
        [FunctionName(nameof(SubscriptionHttpTrigger.SubscribeAsync))]
        public async Task<IActionResult> SubscribeAsync(
            [HttpTrigger(AuthorizationLevel.Function, HttpTriggerKeys.PostMethod, Route = "subscribe")] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            var requestId = (string)req.HttpContext.Items["MS_AzureFunctionsRequestID"];
            var headers = req.Headers.ToDictionary(p => p.Key, p => string.Join("|", p.Value));

            log.LogInformation($"WebSub subscription request was invoked.");
            log.LogInformation($"RequestID: {requestId}");
            log.LogInformation($"Subscription Request Headers: {JsonConvert.SerializeObject(headers, Formatting.Indented)}");

            var payload = default(SubscriptionRequest);
            using (var reader = new StreamReader(req.Body))
            {
                var serialised = await reader.ReadToEndAsync().ConfigureAwait(false);
                payload = JsonConvert.DeserializeObject<SubscriptionRequest>(serialised);
            }

            var response = await this._service.ProcessSubscription(payload).ConfigureAwait(false);

            log.LogInformation($"Subscription Response Headers: {JsonConvert.SerializeObject(response.Headers, Formatting.Indented)}");

            return new StatusCodeResult((int)response.StatusCode);
        }
    }
}
