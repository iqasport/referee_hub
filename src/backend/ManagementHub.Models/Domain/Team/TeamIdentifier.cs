using System;
using ManagementHub.Models.Abstraction;

namespace ManagementHub.Models.Domain.Team;

/// <summary>
/// Identifier of a National Governing Body.
/// In the future it will ensure that it is initialized with the id in the correct format.
/// </summary>
public record struct TeamIdentifier(long Id) : IIdentifiable
{
	private const string IdPrefix = "TM_";

	/// <summary>
	/// Returns the serialized user identifier.
	/// </summary>
	public override string ToString()
	{
		return $"{IdPrefix}{this.Id}";
	}
	/// <summary>
	/// Converts the <paramref name="value"/> into <paramref name="result"/> if it matches the expected format.
	/// </summary>
	/// <returns>True if conversion succeeded, false otherwise.</returns>
	public static bool TryParse(string value, out TeamIdentifier result)
	{
		result = default;

		if (!value.StartsWith(IdPrefix, StringComparison.OrdinalIgnoreCase))
			return false;

		var id = long.Parse(value.AsSpan().Slice(IdPrefix.Length));
		if (id == default)
			return false;

		result = new TeamIdentifier(id);
		return true;
	}

	public static TeamIdentifier Parse(string value) => TryParse(value, out TeamIdentifier result) ? result : throw new FormatException($"The string is not a valid {nameof(TeamIdentifier)}");
}
