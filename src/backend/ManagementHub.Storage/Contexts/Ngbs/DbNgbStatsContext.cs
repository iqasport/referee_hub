using System;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Enums;
using ManagementHub.Processing.Domain.Tests.Extensions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Contexts.Ngbs;

using System.Collections.Generic;
using ManagementHub.Models.Data;
using Microsoft.Extensions.Internal;

public class DbNgbStatsContext : INgbStatsContext
{
	public int TotalRefereesCount { get; set; }

	public int HeadRefereesCount { get; set; }

	public int AssistantRefereesCount { get; set; }

	public int FlagRefereesCount { get; set; }

	public int ScorekeeperRefereesCount { get; set; }

	public int UncertifiedRefereesCount { get; set; }

	public int CompetitiveTeamsCount { get; set; }

	public int DevelopingTeamsCount { get; set; }

	public int InactiveTeamsCount { get; set; }

	public int YouthTeamsCount { get; set; }

	public int UniversityTeamsCount { get; set; }

	public int CommunityTeamsCount { get; set; }

	public int TotalTeamsCount { get; set; }

	public DateTime CollectedAt { get; set; }
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
		IQueryable<NationalGoverningBody> ngbs = this.dbContext.NationalGoverningBodies.AsNoTracking().WithIdentifier(ngb);
		IQueryable<RefereeLocation> locationsInNgb = this.dbContext.RefereeLocations.AsNoTracking()
			.Join(ngbs, loc => loc.NationalGoverningBodyId, ngb => ngb.Id, (loc, _) => loc);
		IQueryable<long> refereeIds = this.dbContext.ActiveUsers(new SystemClock(), CurrentVersion)
			.Join(locationsInNgb, u => u.Id, l => l.RefereeId, (u, l) => u)
			.Select(u => u.Id);
		var refereeCount = await refereeIds.CountAsync();

		IQueryable<Certification> currentCertifications = this.dbContext.Certifications.AsNoTracking().Where(c => c.Version == CurrentVersion);
		var refsCertsLevelValues = await this.dbContext.RefereeCertifications.AsNoTracking()
			.Join(refereeIds, rc => rc.RefereeId, id => id, (rc, _) => rc)
			.Join(currentCertifications, rc => rc.CertificationId, c => c.Id, (rc, c) => new { rc.RefereeId, c.Level })
			.GroupBy(g => g.RefereeId)
			.Select(g => new { RefereeId = g.Key, HighestLevel = g.Select(rc => rc.Level).Max() })
			.ToDictionaryAsync(g => g.RefereeId, g => g.HighestLevel);

		var teamsByAffiliation = await this.dbContext.Teams.AsNoTracking()
			.Join(ngbs, t => t.NationalGoverningBodyId, n => n.Id, (t, _) => t)
			.GroupBy(t => t.GroupAffiliation)
			.Select(g => new { GroupAffiliation = g.Key, Count = g.Count() })
			.Where(g => g.GroupAffiliation != null)
			.ToDictionaryAsync(g => g.GroupAffiliation!.Value, g => g.Count);

		var teamsByStatus = await this.dbContext.Teams.AsNoTracking()
			.Join(ngbs, t => t.NationalGoverningBodyId, n => n.Id, (t, _) => t)
			.GroupBy(t => t.Status)
			.Select(g => new { Status = g.Key, Count = g.Count() })
			.Where(g => g.Status != null)
			.ToDictionaryAsync(g => g.Status!.Value, g => g.Count);

		var refsCertsLevels = refsCertsLevelValues.GroupBy(r => r.Value).ToDictionary(g => g.Key, g => g.Count());

		return new DbNgbStatsContext
		{
			CollectedAt = DateTime.UtcNow,

			TotalRefereesCount = refereeCount,
			HeadRefereesCount = refsCertsLevels.GetValueOrDefault(CertificationLevel.Head),
			FlagRefereesCount = refsCertsLevels.GetValueOrDefault(CertificationLevel.Flag),
			AssistantRefereesCount = refsCertsLevels.GetValueOrDefault(CertificationLevel.Assistant),
			ScorekeeperRefereesCount = refsCertsLevels.GetValueOrDefault(CertificationLevel.Scorekeeper),
			UncertifiedRefereesCount = refereeCount - refsCertsLevels.Values.Sum(),

			CommunityTeamsCount = teamsByAffiliation.GetValueOrDefault(TeamGroupAffiliation.Community),
			UniversityTeamsCount = teamsByAffiliation.GetValueOrDefault(TeamGroupAffiliation.University),
			YouthTeamsCount = teamsByAffiliation.GetValueOrDefault(TeamGroupAffiliation.Youth),

			CompetitiveTeamsCount = teamsByStatus.GetValueOrDefault(TeamStatus.Competitive),
			DevelopingTeamsCount = teamsByStatus.GetValueOrDefault(TeamStatus.Developing),
			InactiveTeamsCount = teamsByStatus.GetValueOrDefault(TeamStatus.Inactive),

			TotalTeamsCount = teamsByStatus.Values.Sum(),
		};
	}

	public async Task<IOrderedEnumerable<INgbStatsContext>> GetHistoricalNgbStatsAsync(NgbIdentifier ngb)
	{
		IQueryable<NationalGoverningBody> ngbs = this.dbContext.NationalGoverningBodies.AsNoTracking().WithIdentifier(ngb);
		IQueryable<NationalGoverningBodyStat> stats = this.dbContext.NationalGoverningBodyStats.AsNoTracking()
			.Join(ngbs, s => s.NationalGoverningBodyId, n => n.Id, (s, _) => s)
			.Where(s => s.EndTime > DateTime.UtcNow.AddMonths(-12)) // collect data for the last 12 months
			.OrderByDescending(s => s.EndTime); // latest first

		var materializedStats = await stats.Select(s => new DbNgbStatsContext
		{
			CollectedAt = s.EndTime ?? DateTime.UtcNow,

			TotalRefereesCount = s.TotalRefereesCount ?? 0,
			HeadRefereesCount = s.HeadRefereesCount ?? 0,
			FlagRefereesCount = s.SnitchRefereesCount ?? 0,
			AssistantRefereesCount = s.AssistantRefereesCount ?? 0,
			ScorekeeperRefereesCount = s.ScorekeeperRefereesCount ?? 0,
			UncertifiedRefereesCount = s.UncertifiedCount ?? 0,

			CommunityTeamsCount = s.CommunityTeamsCount ?? 0,
			UniversityTeamsCount = s.UniversityTeamsCount ?? 0,
			YouthTeamsCount = s.YouthTeamsCount ?? 0,

			CompetitiveTeamsCount = s.CompetitiveTeamsCount ?? 0,
			DevelopingTeamsCount = s.DevelopingTeamsCount ?? 0,
			InactiveTeamsCount = s.InactiveTeamsCount ?? 0,

			TotalTeamsCount = s.TotalTeamsCount ?? 0,
		}).ToListAsync();

		return materializedStats.OrderByDescending(s => s.CollectedAt);
	}
}
