using System;

namespace ManagementHub.Models.Domain.Team;

/// <summary>
/// Public identifier for team invitation records.
/// </summary>
public record struct TeamInvitationIdentifier(long Id)
{
	private const string IdPrefix = "TI_";

	public override string ToString()
	{
		return $"{IdPrefix}{this.Id}";
	}

	public static bool TryParse(string value, out TeamInvitationIdentifier result)
	{
		result = default;

		if (!value.StartsWith(IdPrefix, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		if (!long.TryParse(value.AsSpan().Slice(IdPrefix.Length), out var id))
		{
			return false;
		}

		if (id == default)
		{
			return false;
		}

		result = new TeamInvitationIdentifier(id);
		return true;
	}

	public static TeamInvitationIdentifier Parse(string value) =>
		TryParse(value, out TeamInvitationIdentifier result)
			? result
			: throw new FormatException($"The string is not a valid {nameof(TeamInvitationIdentifier)}");
}
