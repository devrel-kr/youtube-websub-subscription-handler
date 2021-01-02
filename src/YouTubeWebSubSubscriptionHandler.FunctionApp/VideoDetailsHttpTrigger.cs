using System;
using System.IO;
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
    /// This represents the HTTP trigger entity to fetch YouTube video details.
    /// </summary>
    public class VideoDetailsHttpTrigger
    {
        private readonly IFetchService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoDetailsHttpTrigger" /> class.
        /// </summary>
        /// <param name="service"><see cref="IFetchService" /> instance.</param>
        public VideoDetailsHttpTrigger(IFetchService service)
        {
            this._service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Invokes the fetch request handler.
        /// </summary>
        /// <param name="req"><see cref="HttpRequest" /> instance.</param>
        /// <param name="context"><see cref="ExecutionContext" /> instance.</param>
        /// <param name="log"><see cref="ILogger" /> instance.</param>
        /// <returns>Returns the <see cref="IActionResult" /> instance.</returns>
        [FunctionName(nameof(VideoDetailsHttpTrigger.FetchAsync))]
        public async Task<IActionResult> FetchAsync(
            [HttpTrigger(AuthorizationLevel.Function, HttpTriggerKeys.PostMethod, Route = "fetch")] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var payload = default(FetchRequest);
            using (var reader = new StreamReader(req.Body))
            {
                var serialised = await reader.ReadToEndAsync().ConfigureAwait(false);
                payload = JsonConvert.DeserializeObject<FetchRequest>(serialised);
            }

            var item = await this._service.ExtractVideoDetailsAsync(payload).ConfigureAwait(false);
            var response = await this._service.FetchVideoDetailsAsync(item).ConfigureAwait(false);

            return new OkObjectResult(response);
        }
    }
}
