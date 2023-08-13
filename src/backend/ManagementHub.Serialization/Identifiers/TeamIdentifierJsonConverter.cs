using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Serialization.Identifiers;

public class TeamIdentifierJsonConverter : JsonConverter<TeamIdentifier>
{
	public override bool HandleNull => false;
	public override TeamIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? currentElement = reader.GetString();
		if (currentElement is null || !TeamIdentifier.TryParse(currentElement, out TeamIdentifier userId))
		{
			throw new JsonException($"Could not read a {nameof(TeamIdentifier)}.");
		}
		return userId;
	}

	public override void Write(Utf8JsonWriter writer, TeamIdentifier value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
