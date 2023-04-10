using System.Linq;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Storage.Extensions;
public static class NgbCollectionExtensions
{
	public static IQueryable<NationalGoverningBody> WithConstraint(this IQueryable<NationalGoverningBody> ngbs, NgbConstraint constraint)
	{
		if (constraint.AppliesToAny()) return ngbs;
		return ngbs.Where(ngb => constraint.Select(c => c.Id).Contains(ngb.Id));
	}
}
