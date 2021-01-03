using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This specifies the verification type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubscriptionVerificationType
    {
        /// <summary>
        /// Identifies 'undefined'.
        /// </summary>
        [EnumMember(Value = "undefined")]
        Undefined = 0,

        /// <summary>
        /// Identifies 'async'.
        /// </summary>
        [EnumMember(Value = "async")]
        Asynchronous = 1,

        /// <summary>
        /// Identifies 'sync'.
        /// </summary>
        [EnumMember(Value = "sync")]
        Synchronous = 2
    }
}
