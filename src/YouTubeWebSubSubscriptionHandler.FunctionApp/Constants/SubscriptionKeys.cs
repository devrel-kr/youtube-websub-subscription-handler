namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Constants
{
    /// <summary>
    /// This specifies the keys used for subscriptions.
    /// </summary>
    public static class SubscriptionKeys
    {
        /// <summary>
        /// Identifies 'hub.callback'.
        /// </summary>
        public const string HubCallback = "hub.callback";

        /// <summary>
        /// Identifies 'hub.topic'.
        /// </summary>
        public const string HubTopic = "hub.topic";

        /// <summary>
        /// Identifies 'hub.verify'.
        /// </summary>
        public const string HubVerify = "hub.verify";

        /// <summary>
        /// Identifies 'hub.mode'.
        /// </summary>
        public const string HubMode = "hub.mode";

        /// <summary>
        /// Identifies 'hub.challenge'.
        /// </summary>
        public const string HubChallenge = "hub.challenge";

        /// <summary>
        /// Identifies 'hub.verify_token'.
        /// </summary>
        public const string HubVerifyToken = "hub.verify_token";

        /// <summary>
        /// Identifies 'hub.secret'.
        /// </summary>
        public const string HubSecret = "hub.secret";

        /// <summary>
        /// Identifies 'hub.lease_seconds'.
        /// </summary>
        public const string HubLeaseSeconds = "hub.lease_seconds";
    }
}
