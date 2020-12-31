using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Constants;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Extensions;
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
    /// This represents the HTTP trigger entity to handle callback requests.
    /// </summary>
    public class CallbackHttpTrigger
    {
        private readonly ICallbackService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackHttpTrigger" /> class.
        /// </summary>
        /// <param name="service"><see cref="ICallbackService" /> instance.</param>
        public CallbackHttpTrigger(ICallbackService service)
        {
            this._service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Invokes the callback request handler.
        /// </summary>
        /// <param name="req"><see cref="HttpRequest" /> instance.</param>
        /// <param name="context"><see cref="ExecutionContext" /> instance.</param>
        /// <param name="log"><see cref="ILogger" /> instance.</param>
        /// <returns>Returns the <see cref="IActionResult" /> instance.</returns>
        [FunctionName(nameof(CallbackHttpTrigger.CallbackAsync))]
        public async Task<IActionResult> CallbackAsync(
            [HttpTrigger(AuthorizationLevel.Function, HttpTriggerKeys.GetMethod, HttpTriggerKeys.PostMethod, Route = "callback")] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            var requestId = (string)req.HttpContext.Items["MS_AzureFunctionsRequestID"];
            var headers = req.Headers.ToDictionary(p => p.Key, p => string.Join("|", p.Value));

            log.LogInformation($"WebSub subscription callback was invoked.");
            log.LogInformation($"RequestID: {requestId}");
            log.LogInformation($"Callback Request Headers: {JsonConvert.SerializeObject(headers, Formatting.Indented)}");

            var method = req.Method.ToUpperInvariant();

            var verificationResult = await this._service.ProcessVerificationAsync(method, req.Query).ConfigureAwait(false);
            if (verificationResult != null)
            {
                log.LogInformation($"Request verification for {verificationResult.Mode.ToValueString()} has {((int)verificationResult.StatusCode < 400 ? string.Empty : "NOT ")}been processed");

                return new ObjectResult(verificationResult.Challenge) { StatusCode = (int)verificationResult.StatusCode };
            }

            var links = headers[CallbackKeys.LinkHeader]
                        .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim().Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        .ToDictionary(p => p.Last().Trim(), p => p.First().Trim().Replace("<", string.Empty).Replace(">", string.Empty));

            var payload = default(string);
            using (var reader = new StreamReader(req.Body))
            {
                payload = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            var eventResult = await this._service.ProcessEventAsync(method, payload, links).ConfigureAwait(false);
            if (eventResult == null)
            {
                log.LogInformation($"Event has NOT been processed");

                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            log.LogInformation($"Event has {((int)eventResult.StatusCode < 400 ? string.Empty : "NOT ")}been processed");
            log.LogInformation($"Event Response Headers: {JsonConvert.SerializeObject(eventResult.Headers, Formatting.Indented)}");

            return new StatusCodeResult((int)eventResult.StatusCode);
        }
    }
}
