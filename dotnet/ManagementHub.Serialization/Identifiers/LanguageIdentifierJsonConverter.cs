using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Language;

namespace ManagementHub.Serialization.Identifiers;

public class LanguageIdentifierJsonConverter : JsonConverter<LanguageIdentifier>
{
	public override LanguageIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? currentElement = reader.GetString();
		if (currentElement is null || !LanguageIdentifier.TryParse(currentElement, out LanguageIdentifier userId))
		{
			throw new JsonException($"Could not read a {nameof(LanguageIdentifier)}.");
		}
		return userId;
	}

	public override void Write(Utf8JsonWriter writer, LanguageIdentifier value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
