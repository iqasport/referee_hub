using System;

namespace ManagementHub.Models.Domain.Tournament;

public record struct TournamentIdentifier(Ulid UniqueId)
{
	private const string IdPrefix = "TR_";
	private const int UlidAsStringLength = 26;

	/// <summary>
	/// Returns the serialized tournament identifier.
	/// </summary>
	public override string ToString()
	{
		return $"{IdPrefix}{this.UniqueId}";
	}

	/// <summary>
	/// Creates a new unique tournament identifier.
	/// </summary>
	public static TournamentIdentifier NewTournamentId() => new TournamentIdentifier(Ulid.NewUlid());

	/// <summary>
	/// Converts the <paramref name="value"/> into <paramref name="result"/> if it matches the expected format.
	/// </summary>
	/// <returns>True if conversion succeeded, false otherwise.</returns>
	public static bool TryParse(string value, out TournamentIdentifier result)
	{
		result = default;

		if (value.Length != UlidAsStringLength + IdPrefix.Length)
			return false;

		if (!value.StartsWith(IdPrefix, StringComparison.OrdinalIgnoreCase))
			return false;

		if (!Ulid.TryParse(value.AsSpan().Slice(IdPrefix.Length), out var uniqueId))
			return false;

		result = new TournamentIdentifier(uniqueId);
		return true;
	}

	public static TournamentIdentifier Parse(string value) => TryParse(value, out TournamentIdentifier result) ? result : throw new FormatException($"The string is not a valid {nameof(TournamentIdentifier)}");
}
