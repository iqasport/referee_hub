using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Serialization.Identifiers;

public class TournamentParticipantIdentifierJsonConverter : JsonConverter<TournamentParticipantIdentifier>
{
	public override bool HandleNull => false;

	public override TournamentParticipantIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			string? currentElement = reader.GetString();
			if (currentElement is not null && TournamentParticipantIdentifier.TryParse(currentElement, out var participantId))
			{
				return participantId;
			}

			throw new JsonException($"Could not read a {nameof(TournamentParticipantIdentifier)}.");
		}

		if (reader.TokenType == JsonTokenType.StartObject)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			if (TryReadTeamParticipant(root, out var teamParticipant))
			{
				return teamParticipant;
			}

			if (TryReadRefereeParticipant(root, out var refereeParticipant))
			{
				return refereeParticipant;
			}
		}

		throw new JsonException($"Could not read a {nameof(TournamentParticipantIdentifier)}.");
	}

	public override void Write(Utf8JsonWriter writer, TournamentParticipantIdentifier value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}

	private static bool TryReadTeamParticipant(JsonElement root, out TournamentParticipantIdentifier participantIdentifier)
	{
		participantIdentifier = default;

		if (TryReadParticipantType(root, out var participantType) && participantType != ParticipantType.Team)
		{
			return false;
		}

		if (TryReadTeamIdentifier(root, "teamId", out var teamId))
		{
			participantIdentifier = TournamentParticipantIdentifier.ForTeam(teamId);
			return true;
		}

		if (TryReadTeamIdentifier(root, "participantId", out teamId))
		{
			participantIdentifier = TournamentParticipantIdentifier.ForTeam(teamId);
			return true;
		}

		if (TryParseTeamIdentifierElement(root, out teamId))
		{
			participantIdentifier = TournamentParticipantIdentifier.ForTeam(teamId);
			return true;
		}

		return false;
	}

	private static bool TryReadRefereeParticipant(JsonElement root, out TournamentParticipantIdentifier participantIdentifier)
	{
		participantIdentifier = default;

		if (TryReadParticipantType(root, out var participantType) && participantType != ParticipantType.Referee)
		{
			return false;
		}

		if (TryReadUserIdentifier(root, "userId", out var userId))
		{
			participantIdentifier = TournamentParticipantIdentifier.ForReferee(userId);
			return true;
		}

		if (TryReadUserIdentifier(root, "participantId", out userId))
		{
			participantIdentifier = TournamentParticipantIdentifier.ForReferee(userId);
			return true;
		}

		return false;
	}

	private static bool TryReadParticipantType(JsonElement root, out ParticipantType participantType)
	{
		participantType = default;

		if (root.TryGetProperty("participantType", out var participantTypeElement))
		{
			if (participantTypeElement.ValueKind == JsonValueKind.String)
			{
				var typeString = participantTypeElement.GetString();
				if (typeString is not null && Enum.TryParse(typeString, true, out participantType))
				{
					return true;
				}
			}

			if (participantTypeElement.ValueKind == JsonValueKind.Number && participantTypeElement.TryGetInt32(out var typeNumber))
			{
				participantType = (ParticipantType)typeNumber;
				return true;
			}
		}

		return false;
	}

	private static bool TryReadTeamIdentifier(JsonElement root, string propertyName, out TeamIdentifier teamId)
	{
		teamId = default;

		if (!root.TryGetProperty(propertyName, out var property))
		{
			return false;
		}

		return TryParseTeamIdentifierElement(property, out teamId);
	}

	private static bool TryReadUserIdentifier(JsonElement root, string propertyName, out UserIdentifier userId)
	{
		userId = default;

		if (!root.TryGetProperty(propertyName, out var property))
		{
			return false;
		}

		return TryParseUserIdentifierElement(property, out userId);
	}

	private static bool TryParseTeamIdentifierElement(JsonElement element, out TeamIdentifier teamId)
	{
		teamId = default;

		if (element.ValueKind == JsonValueKind.String)
		{
			var teamIdString = element.GetString();
			return teamIdString is not null && TeamIdentifier.TryParse(teamIdString, out teamId);
		}

		return false;
	}

	private static bool TryParseUserIdentifierElement(JsonElement element, out UserIdentifier userId)
	{
		userId = default;

		if (element.ValueKind == JsonValueKind.String)
		{
			return TryParseUserIdentifierString(element.GetString(), out userId);
		}

		return false;
	}

	private static bool TryParseUserIdentifierString(string? value, out UserIdentifier userId)
	{
		userId = default;
		return value is not null && UserIdentifier.TryParse(value, out userId);
	}
}