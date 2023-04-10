using System;
using System.Collections;
using System.Collections.Generic;

namespace ManagementHub.Models.Domain.Ngb;

/// <summary>
/// An NGB constraint that represents one, multiple, or any NGBs.
/// </summary>
public class NgbConstraint : IEnumerable<NgbIdentifier>
{
    private readonly bool anyNgb;
    private readonly HashSet<NgbIdentifier>? nationalGoverningBodies;
    
    private NgbConstraint() => this.anyNgb = true;

    public NgbConstraint(NgbIdentifier nationalGoverningBody)
        => this.nationalGoverningBodies = new HashSet<NgbIdentifier>{nationalGoverningBody};
    public NgbConstraint(IEnumerable<NgbIdentifier> nationalGoverningBodies)
        => this.nationalGoverningBodies = new HashSet<NgbIdentifier>(nationalGoverningBodies);

    public static NgbConstraint Any = new NgbConstraint();

	public bool AppliesToAny() => this.anyNgb;

    public bool AppliesTo(NgbIdentifier ngbId)
    {
        if (this.anyNgb)
        {
            return true;
        }

        return this.nationalGoverningBodies!.Contains(ngbId);
    }

	public override bool Equals(object? obj)
	{
		return obj is NgbConstraint other &&
			this.anyNgb == other.anyNgb &&
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
		int hashCode = this.anyNgb.GetHashCode();
		if (this.nationalGoverningBodies != null)
			foreach(var ngb in this.nationalGoverningBodies)
				hashCode = HashCode.Combine(hashCode, ngb.GetHashCode());
		return hashCode;
	}

	public IEnumerator<NgbIdentifier> GetEnumerator()
	{
		if (this.anyNgb)
		{
			throw new InvalidOperationException("Can't enumerate ANY constraint.");
		}

		return this.nationalGoverningBodies!.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

	public override string ToString()
	{
		if (this.anyNgb) return "ANY";
		return string.Join(", ", this.nationalGoverningBodies!);
	}
}
