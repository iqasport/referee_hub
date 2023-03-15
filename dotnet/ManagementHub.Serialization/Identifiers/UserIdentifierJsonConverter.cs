using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Serialization.Identifiers;

/// <summary>
/// Serializes a <see cref="UserIdentifier"/> as a string.
/// </summary>
public sealed class UserIdentifierJsonConverter : JsonConverter<UserIdentifier>
{
	public override UserIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? currentElement = reader.GetString();
		if (currentElement is null || !UserIdentifier.TryParse(currentElement, out UserIdentifier userId))
		{
			throw new JsonException($"Could not read a {nameof(UserIdentifier)}.");
		}
		return userId;
	}

	public override void Write(Utf8JsonWriter writer, UserIdentifier value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
