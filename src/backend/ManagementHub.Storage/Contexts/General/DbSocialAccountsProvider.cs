using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Contexts.General;

public class DbSocialAccountsProvider : ISocialAccountsProvider
{
	private readonly ManagementHubDbContext dbContext;

	public DbSocialAccountsProvider(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public async Task<IEnumerable<SocialAccount>> GetTeamSocialAccounts(TeamIdentifier teamId)
	{
		return await this.dbContext.SocialAccounts.AsNoTracking().Where(sa => sa.OwnableType == "Team" && sa.OwnableId == teamId.Id)
			.Select(sa => new SocialAccount(new Uri(sa.Url), sa.AccountType)).ToListAsync();
	}

	public Task<Dictionary<NgbIdentifier, IEnumerable<SocialAccount>>> QueryNgbSocialAccounts(NgbConstraint ngbConstraint)
	{
		return this.dbContext.SocialAccounts.AsNoTracking().Where(sa => sa.OwnableType == "NationalGoverningBody")
			.Join(this.dbContext.NationalGoverningBodies.AsNoTracking().WithConstraint(ngbConstraint), sa => sa.OwnableId, ngb => ngb.Id, (sa, ngb) => new { ngb.CountryCode, sa.Url, sa.AccountType })
			.GroupBy(sa => sa.CountryCode)
			.ToDictionaryAsync(g => NgbIdentifier.Parse(g.Key), g => g.Select(sa => new SocialAccount(new Uri(sa.Url), sa.AccountType)));
	}

	public Task<Dictionary<TeamIdentifier, IEnumerable<SocialAccount>>> QueryTeamSocialAccounts(NgbConstraint ngbConstraint)
	{
		var teams = this.dbContext.Teams.Join(this.dbContext.NationalGoverningBodies.AsNoTracking().WithConstraint(ngbConstraint), t => t.NationalGoverningBodyId, n => n.Id, (t, _) => t);
		return this.dbContext.SocialAccounts.AsNoTracking().Where(sa => sa.OwnableType == "Team")
			.Join(teams, sa => sa.OwnableId, t => t.Id, (sa, t) => new { TeamId = t.Id, sa.Url, sa.AccountType })
			.GroupBy(sa => sa.TeamId)
			.ToDictionaryAsync(g => new TeamIdentifier(g.Key), g => g.Select(sa => new SocialAccount(new Uri(sa.Url), sa.AccountType)));
	}

	public async Task<IEnumerable<SocialAccount>> UpdateNgbSocialAccounts(NgbIdentifier ngb, IEnumerable<SocialAccount> socialAccounts)
	{
		var ngbId = await this.dbContext.NationalGoverningBodies.AsNoTracking().WithIdentifier(ngb).Select(ngb => ngb.Id).SingleAsync();
		var socialAccountsList = socialAccounts.ToList();
		var existingSocialAccounts = await this.dbContext.SocialAccounts.Where(sa => sa.OwnableType == "NationalGoverningBody" && sa.OwnableId == ngbId).ToListAsync();
		var socialAccountsToRemove = existingSocialAccounts.Where(sa => !socialAccountsList.Any(s => s.Url.OriginalString == sa.Url && s.Type == sa.AccountType)).ToList();
		var socialAccountsToAdd = socialAccountsList.Where(s => !existingSocialAccounts.Any(sa => sa.Url == s.Url.OriginalString && sa.AccountType == s.Type)).ToList();

		this.dbContext.SocialAccounts.RemoveRange(socialAccountsToRemove);
		this.dbContext.SocialAccounts.AddRange(socialAccountsToAdd.Select(s => new Models.Data.SocialAccount
		{
			OwnableType = "NationalGoverningBody",
			OwnableId = ngbId,
			Url = s.Url.OriginalString,
			AccountType = s.Type
		}));

		await this.dbContext.SaveChangesAsync();

		return await this.dbContext.SocialAccounts.Where(sa => sa.OwnableType == "NationalGoverningBody" && sa.OwnableId == ngbId)
			.Select(sa => new SocialAccount(new Uri(sa.Url), sa.AccountType)).ToListAsync();
	}

	public async Task<IEnumerable<SocialAccount>> UpdateTeamSocialAccounts(TeamIdentifier teamId, IEnumerable<SocialAccount> socialAccounts)
	{
		var socialAccountsList = socialAccounts.ToList();
		var existingSocialAccounts = await this.dbContext.SocialAccounts.Where(sa => sa.OwnableType == "Team" && sa.OwnableId == teamId.Id).ToListAsync();
		var socialAccountsToRemove = existingSocialAccounts.Where(sa => !socialAccountsList.Any(s => s.Url.OriginalString == sa.Url && s.Type == sa.AccountType)).ToList();
		var socialAccountsToAdd = socialAccountsList.Where(s => !existingSocialAccounts.Any(sa => sa.Url == s.Url.OriginalString && sa.AccountType == s.Type)).ToList();

		this.dbContext.SocialAccounts.RemoveRange(socialAccountsToRemove);
		this.dbContext.SocialAccounts.AddRange(socialAccountsToAdd.Select(s => new Models.Data.SocialAccount
		{
			OwnableType = "Team",
			OwnableId = teamId.Id,
			Url = s.Url.OriginalString,
			AccountType = s.Type
		}));

		await this.dbContext.SaveChangesAsync();

		return await this.dbContext.SocialAccounts.Where(sa => sa.OwnableType == "Team" && sa.OwnableId == teamId.Id)
			.Select(sa => new SocialAccount(new Uri(sa.Url), sa.AccountType)).ToListAsync();
	}
}
