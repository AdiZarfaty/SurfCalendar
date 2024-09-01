using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SurfCalendar.Common.Json;

namespace SurfCalendar.Common.DTOs
{
    public abstract class BaseContractAdapter
    {
        private readonly HttpClient _httpClient;

        public BaseContractAdapter()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(GetServiceRootDomain())
            };
        }

        /// <summary>
        /// override and return the microservice root domain
        /// "https://your-microservice-url.com"
        /// </summary>
        /// <returns></returns>
        protected abstract string GetServiceRootDomain();

        private string BuildParamsString(BaseSerializableDto requestParams)
        {
            if (requestParams == null)
            {
                return "";
            }
            var json = requestParams.Serialize();
            var encodedJson = Uri.EscapeDataString(json);

            //TODO - implement params string passing for GET 
            throw new NotImplementedException();

            return $"?requestParams={encodedJson}";
        }

        /// <summary>
        /// HttpMethod.Get
        /// </summary>
        /// <param name="method"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        protected async Task<T> CallMicroserviceAsync<T>(HttpMethod method, string endpoint = "", BaseSerializableDto requestDto = null) where T: JsonDeserializableDto
        {
            try
            {

                // TODO - add authorization

                var endpointWithParams = endpoint;

                // Set parameters on query parameters.
                // GET has no body
                if ((requestDto != null) && (method == HttpMethod.Get))
                {
                    endpointWithParams += BuildParamsString(requestDto);
                }

                // Make the HTTP request
                var request = new HttpRequestMessage(method, endpointWithParams);
                
                // default
                // Set parameters on body.
                if ((requestDto != null) && (method != HttpMethod.Get))
                {
                    request.Content = new StringContent(
                        requestDto.Serialize(),
                        Encoding.UTF8,
                        MediaTypeNames.Application.Json);
                }

                var response = await //_httpClient.GetAsync(endpoint);
                    _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);

                // Ensure a successful response
                response.EnsureSuccessStatusCode();

                // Read the content as a string
                var content = await response.Content.ReadAsStringAsync();

                // Force all my contracts to return json
                var deSerObj = JsonDeserializableDto.DeserializeObject<T>(content);
                return deSerObj;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error calling microservice: {ex.Message}");
                throw;
            }
        }




        
    }
}
