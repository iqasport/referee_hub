using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Tournament;

namespace ManagementHub.Serialization.Identifiers;

/// <summary>
/// Serializes a <see cref="TournamentIdentifier"/> as a string.
/// </summary>
public sealed class TournamentIdentifierJsonConverter : JsonConverter<TournamentIdentifier>
{
	public override bool HandleNull => false;
	public override TournamentIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? currentElement = reader.GetString();
		if (currentElement is null || !TournamentIdentifier.TryParse(currentElement, out TournamentIdentifier tournamentId))
		{
			throw new JsonException($"Could not read a {nameof(TournamentIdentifier)}.");
		}
		return tournamentId;
	}

	public override void Write(Utf8JsonWriter writer, TournamentIdentifier value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
