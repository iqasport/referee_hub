using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManagementHub.Models.Domain.Tournament;

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

	private sealed class TournamentIdentifierJsonConverter : JsonConverter<TournamentIdentifier>
	{
		public override TournamentIdentifier Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
		{
			var value = reader.GetString();
			if (value == null || !TournamentIdentifier.TryParse(value, out var tournamentIdentifier))
			{
				throw new JsonException($"Could not read {nameof(TournamentIdentifier)} value.");
			}

			return tournamentIdentifier;
		}

		public override void Write(Utf8JsonWriter writer, TournamentIdentifier value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}