using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ManagementHub.Models.Domain.Tournament;

/// <summary>
/// A tournament constraint that represents one, multiple, or any tournaments.
/// </summary>
public abstract class TournamentConstraint
{
	public abstract bool AppliesToAny { get; }
	public abstract bool AppliesTo(TournamentIdentifier tournamentId);

	public static TournamentConstraint Any { get; } = new AnyTournamentConstraint();

	public static TournamentConstraint Single(TournamentIdentifier tournamentId) => Set(new[] { tournamentId });

	public static TournamentConstraint Set(IEnumerable<TournamentIdentifier> tournamentIds) => new SetTournamentConstraint(tournamentIds);

	public static TournamentConstraint Empty() => new SetTournamentConstraint(Array.Empty<TournamentIdentifier>());

	public static bool TryParse(string value, [NotNullWhen(true)] out TournamentConstraint? constraint)
	{
		if (value == "ANY")
		{
			constraint = Any;
			return true;
		}
		else if (TournamentIdentifier.TryParse(value, out var id))
		{
			constraint = Single(id);
			return true;
		}

		constraint = default;
		return false;
	}

	public static TournamentConstraint Parse(string value) => TryParse(value, out var constraint) ? constraint : throw new FormatException($"The string is not a valid {nameof(TournamentConstraint)}");

	private sealed class AnyTournamentConstraint : TournamentConstraint
	{
		public override bool AppliesToAny => true;
		public override bool AppliesTo(TournamentIdentifier tournamentId) => true;

		public override bool Equals(object? obj) => obj is AnyTournamentConstraint;
		public override int GetHashCode() => 1;
		public override string ToString() => "ANY";
	}

	private sealed class SetTournamentConstraint : TournamentConstraint, IEnumerable<TournamentIdentifier>
	{
		private readonly HashSet<TournamentIdentifier>? tournaments;

		public SetTournamentConstraint(IEnumerable<TournamentIdentifier> tournaments)
			=> this.tournaments = new HashSet<TournamentIdentifier>(tournaments);

		public override bool AppliesToAny => false;
		public override bool AppliesTo(TournamentIdentifier tournamentId) => this.tournaments!.Contains(tournamentId);

		public override bool Equals(object? obj)
		{
			return obj is SetTournamentConstraint other &&
				(
					(
						this.tournaments != null && other.tournaments != null &&
						this.tournaments.SetEquals(other.tournaments)
					) ||
					this.tournaments == other.tournaments
				);
		}

		public override int GetHashCode()
		{
			int hashCode = 1;
			if (this.tournaments != null)
				foreach (var tournament in this.tournaments)
					hashCode = HashCode.Combine(hashCode, tournament.GetHashCode());
			return hashCode;
		}

		public IEnumerator<TournamentIdentifier> GetEnumerator() => this.tournaments!.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		public override string ToString() => string.Join(", ", this.tournaments!);
	}
}
