using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.User;
using ManagementHub.Serialization.Identifiers;
using ManagementHub.Serialization.Roles;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ManagementHub.Serialization;
public static class DefaultJsonSerialization
{
	public static JsonSerializerOptions ConfigureOptions(JsonSerializerOptions options)
	{
		options.Converters.Add(new UserIdentifierJsonConverter());
		options.Converters.Add(new LanguageIdentifierJsonConverter());
		options.Converters.Add(new UserRoleJsonConverter());

		options.AllowTrailingCommas = true;
		options.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals;
		options.PropertyNameCaseInsensitive = true;
		options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
		options.ReadCommentHandling = JsonCommentHandling.Skip;

		return options;
	}

	public static JsonSerializerOptions Options { get; } = ConfigureOptions(new JsonSerializerOptions());

	public static SwaggerGenOptions MapSwaggerTypes(SwaggerGenOptions options)
	{
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(UserIdentifier), () => new OpenApiSchema { Type = "string" });
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(LanguageIdentifier), () => new OpenApiSchema { Type = "string" });
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(IUserRole), () =>
		{
			return new OpenApiSchema
			{
				Type = "object",
				Properties =
				{
					[UserRoleJsonConverter.RolePropertyName] = new OpenApiSchema { Type = "string" }
				}
			};
		});

		return options;
	}
}
