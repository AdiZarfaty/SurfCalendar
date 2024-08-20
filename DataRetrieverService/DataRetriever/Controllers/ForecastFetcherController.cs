using SurfCalendar.DataRetriever.Contract.Forcast;
using SurfCalendar.DataRetriever.SurflineDtos.Convertion;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace SurfCalendar.DataRetriever.Controllers
{
    [ApiController]
    public class ForecastFetcherController : ControllerBase
    {

        private readonly ILogger<ForecastFetcherController> _logger;

        public ForecastFetcherController(ILogger<ForecastFetcherController> logger)
        {
            _logger = logger;
        }

        [HttpGet("forcast/Test")]
        //[Produces("application/json")]
        public async Task<IActionResult> Test()
        {
            string apiurl = "https://services.surfline.com/kbyg/spots/forecasts";
            string spotId = "584204204e65fad6a7709ab7";//"5842041f4e65fad6a7708906";
            int days = 2;

            var result = await GetDataAsync(apiurl, spotId, days);

            return result;
        }

        [HttpGet("forcast/Data")]
        //[Produces("application/json")]
        public async Task<IActionResult> GetDataAsync(string apiurl = "https://services.surfline.com/kbyg/spots/forecasts", string spotId = "584204204e65fad6a7709ab7", int days = 1)
        {
            var result = await GetRawDataAsync(apiurl, spotId, days);

            SurflineDtos.Forcast.ForcastRoot rawForcast = SurflineDtos.Forcast.ForcastRoot.DeserializeObject<SurflineDtos.Forcast.ForcastRoot>(result);

            var converter = new ForcastConverter();
            ForcastDto myForcast = converter.ConvertForcast(rawForcast);
            return Ok(myForcast.Serialize());
        }

        [HttpGet("forcast/RawData")]
        [Produces("application/text")]
        public async Task<string> GetRawDataAsync(string apiurl, string spotId, int days)
        {

            // URL of the API endpoint
            string apiUrl = apiurl + "?spotId=" + spotId + "&days=" + days;

            // Create an instance of HttpClient
            using var client = new HttpClient();

            try
            {
                // Make the GET request
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                string jsonString = await response.Content.ReadAsStringAsync();

                // Parse the JSON string into a JsonDocument
                using JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement; // check its not empty?

                //// Example: Access some data from the JSON
                //string someValue = root.ToString();  // root.GetProperty("someKey").GetString();
                //Console.WriteLine($"Value: {someValue}");

                // If you know the structure of your JSON, you can also deserialize it into a specific class
                // MyDataClass data = await response.Content.ReadFromJsonAsync<MyDataClass>();
                return jsonString;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HTTP Request Error: {e.Message}");
                throw;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"JSON Parsing Error: {e.Message}");
                throw;
            }
        }
    }

}