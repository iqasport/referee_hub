using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ManagementHub.Service.Swagger;

public class ExternalParameterInBodyFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var attr = context.ApiDescription.CustomAttributes().OfType<ExternalParameterInBodyAttribute>().FirstOrDefault();

		if (attr != null)
		{
			operation.RequestBody = new OpenApiRequestBody();
			operation.RequestBody.Content.Add(attr.MediaType, new OpenApiMediaType()
			{
				Schema = new OpenApiSchema()
				{
					Type = "object",
					Title = attr.Name,
				},
			});
		}
	}
}
