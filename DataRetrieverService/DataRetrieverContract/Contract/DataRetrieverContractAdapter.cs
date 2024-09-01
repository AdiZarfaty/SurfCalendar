using SurfCalendar.Common.DTOs;
using SurfCalendar.Common.Json;
using SurfCalendar.DataRetriever.Contract.Forcast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataRetrieverContract.Contract
{
    public class DataRetrieverContractAdapter : BaseContractAdapter
    {
        protected override string GetServiceRootDomain()
        {
            return "https://localhost:7169/";
        }

        public async Task<ForcastResponseDto> GetForcastDataAsync(string spotId, int days)
        {
            var requestParams = new ForcastRequestDto() 
            { 
                SpotId = spotId,
                Days = days
            };
            ForcastResponseDto response = await CallMicroserviceAsync<ForcastResponseDto>(HttpMethod.Post, "forcast/data", requestParams);
            return response;
        }

    }
}
