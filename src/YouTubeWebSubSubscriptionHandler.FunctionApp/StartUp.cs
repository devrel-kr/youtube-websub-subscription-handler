using System;

using Azure;
using Azure.Messaging.EventGrid;

using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Constants;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models;
using DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Services;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.StartUp))]
namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp
{
    /// <summary>
    /// This represents the entity to be invoked during the runtime startup.
    /// </summary>
    public class StartUp : FunctionsStartup
    {
        /// <inheritdoc />
        public override void Configure(IFunctionsHostBuilder builder)
        {
            this.ConfigureAppSettings(builder.Services);
            this.ConfigureClients(builder.Services);
            this.ConfigureServices(builder.Services);
        }

        private void ConfigureAppSettings(IServiceCollection services)
        {
            services.AddSingleton<AppSettings>();
        }

        private void ConfigureClients(IServiceCollection services)
        {
            var settings = services.BuildServiceProvider().GetService<AppSettings>();

            services.AddHttpClient<ISubscriptionService>();

            var topicEndpoint = new Uri(settings.EventGrid.Topic.Endpoint);
            var credential = new AzureKeyCredential(settings.EventGrid.Topic.AccessKey);
            var publisher = new EventGridPublisherClient(topicEndpoint, credential);

            services.AddSingleton(publisher);

            var initialiser = new BaseClientService.Initializer() { ApplicationName = FetchKeys.UserAgent, ApiKey = settings.YouTube.ApiKey };
            var youtube = new YouTubeService(initialiser);
            var resource = new VideosResource(youtube);

            services.AddSingleton(resource);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ISubscriptionService, SubscriptionService>();
            services.AddTransient<ICallbackService, CallbackService>();
            services.AddTransient<IFetchService, FetchService>();
        }
    }
}
