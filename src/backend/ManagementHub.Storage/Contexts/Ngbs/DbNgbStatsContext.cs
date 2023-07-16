using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Enums;

namespace ManagementHub.Storage.Contexts.Ngbs;
using ManagementHub.Models.Data;
using ManagementHub.Processing.Domain.Tests.Extensions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

public class DbNgbStatsContext : INgbStatsContext
{
	public required Dictionary<CertificationLevel, int> RefereeCountByHighestObtainedLevelForCurrentRulebook { get; set; }

	public required Dictionary<TeamGroupAffiliation, int> TeamCountByGroupAffiliation { get; set; }

	public required Dictionary<TeamStatus, int> TeamCountByStatus { get; set; }
}

public class DbNgbStatsContextFactory
{
	private readonly ManagementHubDbContext dbContext;

	// TODO: this should be sourced from settings or something
	private const CertificationVersion CurrentVersion = CertificationVersion.TwentyTwo;

	public DbNgbStatsContextFactory(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public async Task<DbNgbStatsContext> GetNgbStatsContextAsync(NgbIdentifier ngb)
	{
		IQueryable<NationalGoverningBody> ngbs = this.dbContext.NationalGoverningBodies.WithIdentifier(ngb);
		IQueryable<RefereeLocation> locationsInNgb = this.dbContext.RefereeLocations
			.Join(ngbs, loc => loc.NationalGoverningBodyId, ngb => ngb.Id, (loc, _) => loc);
		IQueryable<long> refereeIds = locationsInNgb.Select(l => l.RefereeId);
		var refereeCountTask = refereeIds.CountAsync();

		IQueryable<Certification> currentCertifications = this.dbContext.Certifications.Where(c => c.Version == CurrentVersion);
		var refsCertsLevelTask = this.dbContext.RefereeCertifications
			.Join(refereeIds, rc => rc.RefereeId, id => id, (rc, _) => rc)
			.Join(currentCertifications, rc => rc.CertificationId, c => c.Id, (rc, c) => new { rc.RefereeId, c.Level })
			.GroupBy(g => g.RefereeId)
			.Select(g => new { RefereeId = g.Key, HighestLevel = g.Select(rc => rc.Level).Max() })
			.ToDictionaryAsync(g => g.RefereeId, g => g.HighestLevel);

		var teamsByAffiliationTask = this.dbContext.Teams
			.Join(ngbs, t => t.NationalGoverningBodyId, n => n.Id, (t, _) => t)
			.GroupBy(t => t.GroupAffiliation)
			.Select(g => new { GroupAffiliation = g.Key, Count = g.Count() })
			.Where(g => g.GroupAffiliation != null)
			.ToDictionaryAsync(g => g.GroupAffiliation!.Value, g => g.Count);

		var teamsByStatusTask = this.dbContext.Teams
			.Join(ngbs, t => t.NationalGoverningBodyId, n => n.Id, (t, _) => t)
			.GroupBy(t => t.Status)
			.Select(g => new { Status = g.Key, Count = g.Count() })
			.Where(g => g.Status != null)
			.ToDictionaryAsync(g => g.Status!.Value, g => g.Count);

		await Task.WhenAll(refereeCountTask, refsCertsLevelTask, teamsByAffiliationTask, teamsByStatusTask);

		var refereeCount = await refereeCountTask;
		var refsCertsLevels = (await refsCertsLevelTask).GroupBy(r => r.Value).ToDictionary(g => g.Key, g => g.Count());
		var teamsByAffiliation = await teamsByAffiliationTask;
		var teamsByStatus = await teamsByStatusTask;

		refsCertsLevels.Add((CertificationLevel)(-1), refereeCount - refsCertsLevels.Values.Sum());

		return new DbNgbStatsContext
		{
			RefereeCountByHighestObtainedLevelForCurrentRulebook = refsCertsLevels,
			TeamCountByGroupAffiliation = teamsByAffiliation,
			TeamCountByStatus = teamsByStatus,
		};
	}
}
