using Newtonsoft.Json;

namespace SurfCalendar.Common.Json
{
    public abstract class JsonDeserializableDto
    {
        /// <summary>
        /// Original string obj was deserialized from
        /// </summary>
        private string? _DebugDeserializeOrigin;

        public static T DeserializeObject<T>(string origin) where T : JsonDeserializableDto
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                throw new ArgumentException(nameof(origin));
            }

            T deserializedClass = JsonConvert.DeserializeObject<T>(origin)!;
            deserializedClass!._DebugDeserializeOrigin = origin;
            return deserializedClass;
        }
    }
}
