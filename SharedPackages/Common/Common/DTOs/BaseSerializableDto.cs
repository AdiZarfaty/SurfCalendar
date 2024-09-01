using SurfCalendar.Common.Json;

namespace SurfCalendar.Common.DTOs
{
    public abstract class BaseSerializableDto : JsonDeserializableDto
    {
        public string Serialize()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}
