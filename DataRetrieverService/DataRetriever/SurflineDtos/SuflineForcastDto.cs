using SurfCalendar.Common.Json;

namespace SurfCalendar.DataRetriever.SurflineDtos.Forcast
{
    // parsed with https://json2csharp.com/
    // usage:
    // var myDeserializedClass = JsonConvert.DeserializeObject<SuflineForcastDto>(myJsonResponse);
    public class ForcastRoot : JsonDeserializableDto
    {
        public Associated associated { get; set; }
        public Units units { get; set; }
        public int utcOffset { get; set; }
        public Data data { get; set; }
        public Permissions permissions { get; set; }
    }

    public class Associated
    {
        public Units units { get; set; }
        public int utcOffset { get; set; }
        public string weatherIconPath { get; set; }
    }

    public class Data
    {
        public List<SunriseSunsetTime> sunriseSunsetTimes { get; set; }
        public TideLocation tideLocation { get; set; }
        public List<ForecastPoint> forecasts { get; set; }
        public List<Tide> tides { get; set; }
    }

    /// <summary>
    /// named just "Forecast" on API
    /// </summary>
    public class ForecastPoint
    {
        public int timestamp { get; set; }
        public Weather weather { get; set; }
        public Wind wind { get; set; }
        public Surf surf { get; set; }
        public List<Swell> swells { get; set; }
    }

    public class Permission
    {
        public string name { get; set; }
        public bool required { get; set; }
    }

    public class Permissions
    {
        public List<object> data { get; set; }
        public List<Violation> violations { get; set; }
    }

    public class SunriseSunsetTime
    {
        public int midnight { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class Surf
    {
        public double min { get; set; }
        public double max { get; set; }
    }

    public class Swell
    {
        public double height { get; set; }
        public double direction { get; set; }
        public double directionMin { get; set; }
        public int period { get; set; }
    }

    public class Tide
    {
        public int timestamp { get; set; }
        public string type { get; set; }
        public double height { get; set; }
    }

    public class TideLocation
    {
        public string name { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public double lon { get; set; }
        public double lat { get; set; }
        public int mean { get; set; }
    }

    public class Units
    {
        public string temperature { get; set; }
        public string tideHeight { get; set; }
        public string swellHeight { get; set; }
        public string waveHeight { get; set; }
        public string windSpeed { get; set; }
        public string pressure { get; set; }
    }

    public class Violation
    {
        public int code { get; set; }
        public string message { get; set; }
        public Permission permission { get; set; }
    }

    public class Weather
    {
        public int temperature { get; set; }
        public string condition { get; set; }
    }

    public class Wind
    {
        public double speed { get; set; }
        public double direction { get; set; }
    }


}
