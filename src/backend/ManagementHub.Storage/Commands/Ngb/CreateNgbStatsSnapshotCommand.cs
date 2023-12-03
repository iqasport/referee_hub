using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Misc;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Ngb;
public class CreateNgbStatsSnapshotCommand : ICreateNgbStatsSnapshotCommand
{
	private readonly ManagementHubDbContext dbContext;
	private readonly INgbContextProvider ngbContextProvider;
	private readonly ILogger<CreateNgbStatsSnapshotCommand> logger;

	public CreateNgbStatsSnapshotCommand(ManagementHubDbContext dbContext, INgbContextProvider ngbContextProvider, ILogger<CreateNgbStatsSnapshotCommand> logger)
	{
		this.dbContext = dbContext;
		this.ngbContextProvider = ngbContextProvider;
		this.logger = logger;
	}

	public async Task CreateNgbStatsSnapshot(NgbConstraint ngbs, CancellationToken cancellationToken)
	{
		using var activity = ActivityExtensions.Source.StartActivity(nameof(CreateNgbStatsSnapshotCommand));

		var ngbIds = this.dbContext.NationalGoverningBodies.AsNoTracking().WithConstraint(ngbs).Select(ngb => ValueTuple.Create(NgbIdentifier.Parse(ngb.CountryCode), ngb.Id)).ToList();

		this.logger.LogInformation(-0x9faad00, "Creating stats snapshot for ngbs: ", ngbIds.Select(x => x.Item1));

		foreach (var (ngbId, ngbDbId) in ngbIds)
		{
			var stats = await this.ngbContextProvider.GetCurrentNgbStatsAsync(ngbId);

			this.dbContext.NationalGoverningBodyStats.Add(new NationalGoverningBodyStat
			{
				AssistantRefereesCount = stats.AssistantRefereesCount,
				CommunityTeamsCount = stats.CommunityTeamsCount,
				CompetitiveTeamsCount = stats.CompetitiveTeamsCount,
				DevelopingTeamsCount = stats.DevelopingTeamsCount,
				HeadRefereesCount = stats.HeadRefereesCount,
				InactiveTeamsCount = stats.InactiveTeamsCount,
				ScorekeeperRefereesCount = stats.ScorekeeperRefereesCount,
				SnitchRefereesCount = stats.FlagRefereesCount,
				TotalRefereesCount = stats.TotalRefereesCount,
				TotalTeamsCount = stats.TotalTeamsCount,
				UncertifiedCount = stats.UncertifiedRefereesCount,
				UniversityTeamsCount = stats.UniversityTeamsCount,
				YouthTeamsCount = stats.YouthTeamsCount,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				EndTime = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1).ToDateTime(new TimeOnly(23, 59), DateTimeKind.Utc),
				NationalGoverningBodyId = ngbDbId,
			});
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);
	}
}
