using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Tournament;

namespace ManagementHub.Serialization.Identifiers;

/// <summary>
/// Serializes a <see cref="TournamentConstraint"/> as a string or array of strings.
/// </summary>
public sealed class TournamentConstraintJsonConverter : JsonConverter<TournamentConstraint>
{
	public override bool HandleNull => false;
	public override TournamentConstraint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, TournamentConstraint value, JsonSerializerOptions options)
	{
		if (value.AppliesToAny)
		{
			writer.WriteStringValue("ANY");
		}
		else if (value is IEnumerable<TournamentIdentifier> tournamentIds)
		{
			var tournamentIdsList = tournamentIds.ToList();
			if (tournamentIdsList.Count == 1)
			{
				writer.WriteStringValue(tournamentIdsList.First().ToString());
			}
			else
			{
				writer.WriteStartArray();
				foreach (var tournamentId in tournamentIdsList)
				{
					writer.WriteStringValue(tournamentId.ToString());
				}
				writer.WriteEndArray();
			}
		}
	}
}
