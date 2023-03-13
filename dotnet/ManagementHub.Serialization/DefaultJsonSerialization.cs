using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Serialization.Identifiers;

namespace ManagementHub.Serialization;
public static class DefaultJsonSerialization
{
	public static JsonSerializerOptions ConfigureOptions(JsonSerializerOptions options)
	{
		options.Converters.Add(new UserIdentifierJsonConverter());
		options.Converters.Add(new LanguageIdentifierJsonConverter());

		options.AllowTrailingCommas = true;
		options.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals;
		options.PropertyNameCaseInsensitive = true;
		options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
		options.ReadCommentHandling = JsonCommentHandling.Skip;

		return options;
	}

	public static JsonSerializerOptions Options { get; } = ConfigureOptions(new JsonSerializerOptions());
}
