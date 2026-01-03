using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Team;

namespace ManagementHub.Serialization.Identifiers;

/// <summary>
/// Serializes a <see cref="TeamConstraint"/> as a string or array of strings.
/// </summary>
public sealed class TeamConstraintJsonConverter : JsonConverter<TeamConstraint>
{
	public override bool HandleNull => false;
	public override TeamConstraint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, TeamConstraint value, JsonSerializerOptions options)
	{
		if (value.AppliesToAny)
		{
			writer.WriteStringValue("ANY");
		}
		else if (value is IEnumerable<TeamIdentifier> teamIds)
		{
			var teamIdsList = teamIds.ToList();
			if (teamIdsList.Count == 1)
			{
				writer.WriteStringValue(teamIdsList.First().ToString());
			}
			else
			{
				writer.WriteStartArray();
				foreach (var teamId in teamIdsList)
				{
					writer.WriteStringValue(teamId.ToString());
				}
				writer.WriteEndArray();
			}
		}
	}
}
