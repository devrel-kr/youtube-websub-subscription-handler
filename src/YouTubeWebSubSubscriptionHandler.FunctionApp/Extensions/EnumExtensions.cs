using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Extensions
{
    /// <summary>
    /// This represents the extension entity for enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the display name of the enum value.
        /// </summary>
        /// <param name="enum">Enum value.</param>
        /// <returns>Display name of the enum value.</returns>
        public static string ToValueString(this Enum @enum)
        {
            var type = @enum.GetType();
            var member = type.GetMember(@enum.ToString()).First();
            var attribute = member.GetCustomAttribute<EnumMemberAttribute>(inherit: false);
            var name = attribute == null ? @enum.ToString() : attribute.Value;

            return name;
        }
    }
}
