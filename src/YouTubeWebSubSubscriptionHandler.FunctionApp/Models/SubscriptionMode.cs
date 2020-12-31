using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This specifies the subscription mode.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubscriptionMode
    {
        /// <summary>
        /// Identifies 'undefined'.
        /// </summary>
        [EnumMember(Value = "undefined")]
        Undefined = 0,

        /// <summary>
        /// Identifies 'unsubscribe'.
        /// </summary>
        [EnumMember(Value = "unsubscribe")]
        Unsubscribe = 1,

        /// <summary>
        /// Identifies 'subscribe'.
        /// </summary>
        [EnumMember(Value = "subscribe")]
        Subscribe = 2
    }
}
