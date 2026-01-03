using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ManagementHub.Models.Domain.Team;

/// <summary>
/// A team constraint that represents one, multiple, or any teams.
/// </summary>
public abstract class TeamConstraint
{
	public abstract bool AppliesToAny { get; }
	public abstract bool AppliesTo(TeamIdentifier teamId);

	public static TeamConstraint Any { get; } = new AnyTeamConstraint();

	public static TeamConstraint Single(TeamIdentifier teamId) => Set(new[] { teamId });

	public static TeamConstraint Set(IEnumerable<TeamIdentifier> teamIds) => new SetTeamConstraint(teamIds);

	public static TeamConstraint Empty() => new SetTeamConstraint(Array.Empty<TeamIdentifier>());

	public static bool TryParse(string value, [NotNullWhen(true)] out TeamConstraint? constraint)
	{
		if (value == "ANY")
		{
			constraint = Any;
			return true;
		}
		else if (TeamIdentifier.TryParse(value, out var id))
		{
			constraint = Single(id);
			return true;
		}

		constraint = default;
		return false;
	}

	public static TeamConstraint Parse(string value) => TryParse(value, out var constraint) ? constraint : throw new FormatException($"The string is not a valid {nameof(TeamConstraint)}");

	private sealed class AnyTeamConstraint : TeamConstraint
	{
		public override bool AppliesToAny => true;
		public override bool AppliesTo(TeamIdentifier teamId) => true;

		public override bool Equals(object? obj) => obj is AnyTeamConstraint;
		public override int GetHashCode() => 1;
		public override string ToString() => "ANY";
	}

	private sealed class SetTeamConstraint : TeamConstraint, IEnumerable<TeamIdentifier>
	{
		private readonly HashSet<TeamIdentifier> teams;

		public SetTeamConstraint(IEnumerable<TeamIdentifier> teams)
			=> this.teams = new HashSet<TeamIdentifier>(teams);

		public override bool AppliesToAny => false;
		public override bool AppliesTo(TeamIdentifier teamId) => this.teams.Contains(teamId);

		public override bool Equals(object? obj)
		{
			return obj is SetTeamConstraint other && this.teams.SetEquals(other.teams);
		}

		public override int GetHashCode()
		{
			int hashCode = 1;
			if (this.teams != null)
				foreach (var team in this.teams)
					hashCode = HashCode.Combine(hashCode, team.GetHashCode());
			return hashCode;
		}

		public IEnumerator<TeamIdentifier> GetEnumerator() => this.teams!.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		public override string ToString() => string.Join(", ", this.teams!);
	}
}
