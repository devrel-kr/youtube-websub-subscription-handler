namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Constants
{
    /// <summary>
    /// This specifies the keys used for callbacks.
    /// </summary>
    public static class CallbackKeys
    {
        /// <summary>
        /// Identifies 'rel=self'.
        /// </summary>
        public const string RelSelf = "rel=self";

        /// <summary>
        /// Identifies 'rel=hub'.
        /// </summary>
        public const string RelHub = "rel=hub";

        /// <summary>
        /// Identifies 'application/cloudevents+json'.
        /// </summary>
        public const string DataContentType = "application/cloudevents+json";

        /// <summary>
        /// Identifies '{at:deleted-entry'.
        /// </summary>
        public const string DeletedEntry = "<at:deleted-entry";

        /// <summary>
        /// Identifies 'Link'.
        /// </summary>
        public const string LinkHeader = "Link";
    }
}
