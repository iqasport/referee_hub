using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Collections;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Contexts.Team;

public class DbTeamContextProvider : ITeamContextProvider
{
	private readonly DbTeamContextFactory dbTeamContextFactory;
	private readonly ManagementHubDbContext dbContext;

	public DbTeamContextProvider(ManagementHubDbContext dbContext, CollectionFilteringContext filteringContext)
	{
		this.dbContext = dbContext;
		this.dbTeamContextFactory = new DbTeamContextFactory(
			dbContext,
			filteringContext
		);
	}

	public async Task<bool> CheckTeamExistsInNgbAsync(NgbIdentifier ngb, TeamIdentifier teamId)
	{
		var count = await this.dbTeamContextFactory.QueryTeamsInternal(NgbConstraint.Single(ngb)).Where(t => t.Id == teamId.Id).CountAsync();
		Debug.Assert(count >= 0 && count <= 1);
		return count > 0;
	}

	public Task<ITeamContext> CreateTeamAsync(NgbIdentifier ngb, TeamData teamData)
	{
		return this.dbTeamContextFactory.CreateTeamAsync(ngb, teamData);
	}

	public Task DeleteTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId)
	{
		return this.dbTeamContextFactory.DeleteTeamAsync(teamId);
	}

	public IQueryable<ITeamContext> GetTeams(NgbConstraint ngbs)
	{
		return this.dbTeamContextFactory.QueryTeams(ngbs);
	}

	public Task<ITeamContext> UpdateTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId, TeamData teamData)
	{
		return this.dbTeamContextFactory.UpdateTeamAsync(ngb, teamId, teamData);
	}

	public async Task<ITeamContext?> GetTeamAsync(TeamIdentifier teamId)
	{
		return await this.dbTeamContextFactory.QueryTeamsInternal(NgbConstraint.Any)
			.Where(t => t.Id == teamId.Id)
			.Select(DbTeamContextFactory.Selector)
			.FirstOrDefaultAsync();
	}

	public async Task<IEnumerable<ManagerInfo>> GetTeamManagersAsync(TeamIdentifier teamId)
	{
		var teamDbId = teamId.Id;

		var managers = await this.dbContext.TeamManagers
			.Where(tm => tm.TeamId == teamDbId)
			.Join(
				this.dbContext.Users,
				tm => tm.UserId,
				u => u.Id,
				(tm, u) => new ManagerInfo
				{
					UserId = u.UniqueId != null
						? UserIdentifier.Parse(u.UniqueId)
						: UserIdentifier.FromLegacyUserId(u.Id),
					Name = $"{u.FirstName} {u.LastName}",
					Email = u.Email
				})
			.ToListAsync();

		return managers;
	}
}
