using System;
using System.Collections.Generic;
using System.Linq;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Storage.Extensions;
public static class NgbCollectionExtensions
{
	public static IQueryable<NationalGoverningBody> WithConstraint(this IQueryable<NationalGoverningBody> ngbs, NgbConstraint constraint)
	{
		if (constraint.AppliesToAny) return ngbs;
		var ngbIdentifiers = constraint as IEnumerable<NgbIdentifier> ?? throw new InvalidOperationException("Constraint is not ANY, but not an enumerable?");
		return ngbs.Where(ngb => ngbIdentifiers.Select(c => c.Id).Contains(ngb.Id));
	}
}
