using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Models
{
    /// <summary>
    /// This specifies the YouTube video event type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum YouTubeVideoEventType
    {
        /// <summary>
        /// Identifies 'undefined'.
        /// </summary>
        [EnumMember(Value = "undefined")]
        Undefined = 0,

        /// <summary>
        /// Identifies 'published'.
        /// </summary>
        [EnumMember(Value = "com.youtube.video.unpublished")]
        Unpublished = 1,

        /// <summary>
        /// Identifies 'unpublished'.
        /// </summary>
        [EnumMember(Value = "com.youtube.video.published")]
        Published = 2
    }
}
