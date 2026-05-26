using System.Text.Json;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Serialization.Identifiers;

namespace ManagementHub.Serialization.Tournaments;

public static class PublicTournamentSnapshotJson
{
	private static readonly JsonSerializerOptions Options = CreateOptions();

	public static string Serialize(PublicTournamentSnapshotPayload payload)
	{
		return JsonSerializer.Serialize(payload, Options);
	}

	public static PublicTournamentSnapshotPayload? Deserialize(string snapshotJson)
	{
		return JsonSerializer.Deserialize<PublicTournamentSnapshotPayload>(snapshotJson, Options);
	}

	private static JsonSerializerOptions CreateOptions()
	{
		var options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};

		options.Converters.Add(new TournamentIdentifierJsonConverter());
		return options;
	}
}