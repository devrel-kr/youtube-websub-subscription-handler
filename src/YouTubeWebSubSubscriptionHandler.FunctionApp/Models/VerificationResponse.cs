namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This represents the response entity for subscription request verification.
    /// </summary>
    public class VerificationResponse : Response
    {
        /// <summary>
        /// Gets or sets the challenge code.
        /// </summary>
        public virtual string Challenge { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SubscriptionMode" /> value.
        /// </summary>
        public virtual SubscriptionMode Mode { get; set; } = SubscriptionMode.Undefined;
    }
}
