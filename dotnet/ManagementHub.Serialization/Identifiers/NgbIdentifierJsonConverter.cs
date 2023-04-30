using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Serialization.Identifiers;

/// <summary>
/// Serializes a <see cref="NgbIdentifier"/> as a string.
/// </summary>
public sealed class NgbIdentifierJsonConverter : JsonConverter<NgbIdentifier>
{
	public override bool HandleNull => false;
	public override NgbIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? currentElement = reader.GetString();
		if (currentElement is null || !NgbIdentifier.TryParse(currentElement, out NgbIdentifier userId))
		{
			throw new JsonException($"Could not read an {nameof(NgbIdentifier)}.");
		}
		return userId;
	}

	public override void Write(Utf8JsonWriter writer, NgbIdentifier value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
