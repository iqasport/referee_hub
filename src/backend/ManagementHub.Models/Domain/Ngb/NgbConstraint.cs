using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ManagementHub.Models.Domain.Ngb;

/// <summary>
/// An NGB constraint that represents one, multiple, or any NGBs.
/// </summary>
public abstract class NgbConstraint
{
	public abstract bool AppliesToAny { get; }
	public abstract bool AppliesTo(NgbIdentifier ngbId);

	public static NgbConstraint Any { get; } = new AnyNgbConstraint();

	public static NgbConstraint Single(NgbIdentifier ngbId) => Set(new[] { ngbId });

	public static NgbConstraint Set(IEnumerable<NgbIdentifier> ngbIds) => new SetNgbConstraint(ngbIds);

	public static bool TryParse(string value, [NotNullWhen(true)] out NgbConstraint? constraint)
	{
		if (value == "ANY")
		{
			constraint = Any;
			return true;
		}
		else if (NgbIdentifier.TryParse(value, out var id))
		{
			constraint = Single(id);
			return true;
		}

		constraint = default;
		return false;
	}

	public static NgbConstraint Parse(string value) => TryParse(value, out var constraint) ? constraint : throw new FormatException($"The string is not a valid {nameof(NgbConstraint)}");

	private sealed class AnyNgbConstraint : NgbConstraint
	{
		public override bool AppliesToAny => true;
		public override bool AppliesTo(NgbIdentifier ngbId) => true;

		public override bool Equals(object? obj) => obj is AnyNgbConstraint;
		public override int GetHashCode() => 1;
		public override string ToString() => "ANY";
	}

	private sealed class SetNgbConstraint : NgbConstraint, IEnumerable<NgbIdentifier>
	{

		private readonly HashSet<NgbIdentifier>? nationalGoverningBodies;

		public SetNgbConstraint(IEnumerable<NgbIdentifier> nationalGoverningBodies)
			=> this.nationalGoverningBodies = new HashSet<NgbIdentifier>(nationalGoverningBodies);

		public override bool AppliesToAny => false;
		public override bool AppliesTo(NgbIdentifier ngbId) => this.nationalGoverningBodies!.Contains(ngbId);

		public override bool Equals(object? obj)
		{
			return obj is SetNgbConstraint other &&
				(
					(
						this.nationalGoverningBodies != null && other.nationalGoverningBodies != null &&
						this.nationalGoverningBodies.SetEquals(other.nationalGoverningBodies)
					) ||
					this.nationalGoverningBodies == other.nationalGoverningBodies // when one of them is null or they're not set equal - this checks that they're both null
				);
		}

		public override int GetHashCode()
		{
			int hashCode = 1;
			if (this.nationalGoverningBodies != null)
				foreach (var ngb in this.nationalGoverningBodies)
					hashCode = HashCode.Combine(hashCode, ngb.GetHashCode());
			return hashCode;
		}

		public IEnumerator<NgbIdentifier> GetEnumerator() => this.nationalGoverningBodies!.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		public override string ToString() => string.Join(", ", this.nationalGoverningBodies!);
	}
}
