using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfCalendar.Common.Swagger
{
    public class ParametersOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var parameter in operation.Parameters)
            {
                if ((parameter.In == ParameterLocation.Header) || (parameter.In == ParameterLocation.Query))
                {
                    // Set the "explode" behavior for header parameters
                    //parameter.Explode = true;
                    //parameter.
                    //parameter.Style = ParameterStyle.Form;
                }
            }
        }
    }
}
