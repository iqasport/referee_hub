using System.Linq;
using ManagementHub.Models.Data;
using ManagementHub.Models.Enums;

namespace ManagementHub.Storage.Extensions;

public static class RefereeCertificationQueryExtensions
{
	public static IOrderedQueryable<RefereeCertification> OrderForRosterDisplay(
		this IQueryable<RefereeCertification> certifications) =>
		certifications
			.Where(rc => rc.ReceivedAt != null && rc.RevokedAt == null)
			.OrderByDescending(rc => rc.Certification.Version.HasValue)
			.ThenByDescending(rc => rc.Certification.Version)
			.ThenByDescending(rc => rc.Certification.Level == CertificationLevel.Field)
			.ThenByDescending(rc => rc.Certification.Level == CertificationLevel.Head)
			.ThenByDescending(rc => rc.Certification.Level == CertificationLevel.Flag)
			.ThenByDescending(rc => rc.Certification.Level == CertificationLevel.Assistant)
			.ThenByDescending(rc => rc.Certification.Level == CertificationLevel.FlagRunner)
			.ThenByDescending(rc => rc.ReceivedAt);
}
