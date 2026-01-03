using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ManagementHub.Models.Abstraction;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.Tournament;
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
		options.Converters.Add(new TestIdentifierJsonConverter());
		options.Converters.Add(new TestAttemptIdentifierJsonConverter());
		options.Converters.Add(new TeamIdentifierJsonConverter());
		options.Converters.Add(new LanguageIdentifierJsonConverter());
		options.Converters.Add(new NgbConstraintJsonConverter());
		options.Converters.Add(new NgbIdentifierJsonConverter());
		options.Converters.Add(new TeamConstraintJsonConverter());
		options.Converters.Add(new TournamentConstraintJsonConverter());
		options.Converters.Add(new TournamentIdentifierJsonConverter());
		options.Converters.Add(new UserRoleJsonConverter());
		options.Converters.Add(new PercentageJsonConverter());
		options.Converters.Add(JsonMetadataServices.TimeSpanConverter);
		options.Converters.Add(JsonMetadataServices.DateOnlyConverter);

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
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(TestIdentifier), () => new OpenApiSchema { Type = "string" });
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(TestAttemptIdentifier), () => new OpenApiSchema { Type = "string" });
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(LanguageIdentifier), () => new OpenApiSchema { Type = "string" });
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(NgbIdentifier), () => new OpenApiSchema { Type = "string" });
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(TeamIdentifier), () => new OpenApiSchema { Type = "string" });
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(TournamentIdentifier), () => new OpenApiSchema { Type = "string" });
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
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(Percentage), () => new OpenApiSchema { Type = "number" });
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(TimeSpan), () => new OpenApiSchema { Type = "string" });
		options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(DateOnly), () => new OpenApiSchema { Type = "string" });

		return options;
	}
}
