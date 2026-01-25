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
		var accounts = await this.dbContext.SocialAccounts.AsNoTracking()
			.Where(sa => sa.OwnableType == "Team" && sa.OwnableId == teamId.Id)
			.ToListAsync();

		return FilterValidSocialAccounts(accounts);
	}

	public async Task<Dictionary<NgbIdentifier, IEnumerable<SocialAccount>>> QueryNgbSocialAccounts(NgbConstraint ngbConstraint)
	{
		var accounts = await this.dbContext.SocialAccounts.AsNoTracking()
			.Where(sa => sa.OwnableType == "NationalGoverningBody")
			.Join(this.dbContext.NationalGoverningBodies.AsNoTracking().WithConstraint(ngbConstraint),
				sa => sa.OwnableId,
				ngb => ngb.Id,
				(sa, ngb) => new { ngb.CountryCode, sa.Url, sa.AccountType })
			.ToListAsync();

		// Group and filter out invalid URLs
		var result = new Dictionary<NgbIdentifier, IEnumerable<SocialAccount>>();
		foreach (var group in accounts.GroupBy(a => a.CountryCode))
		{
			var groupAccounts = group.Select(a => new Models.Data.SocialAccount { Url = a.Url, AccountType = a.AccountType }).ToList();
			result[NgbIdentifier.Parse(group.Key)] = FilterValidSocialAccounts(groupAccounts);
		}
		return result;
	}

	public async Task<Dictionary<TeamIdentifier, IEnumerable<SocialAccount>>> QueryTeamSocialAccounts(NgbConstraint ngbConstraint)
	{
		var teams = this.dbContext.Teams.Join(
			this.dbContext.NationalGoverningBodies.AsNoTracking().WithConstraint(ngbConstraint),
			t => t.NationalGoverningBodyId,
			n => n.Id,
			(t, _) => t);

		var accounts = await this.dbContext.SocialAccounts.AsNoTracking()
			.Where(sa => sa.OwnableType == "Team")
			.Join(teams, sa => sa.OwnableId, t => t.Id, (sa, t) => new { TeamId = t.Id, sa.Url, sa.AccountType })
			.ToListAsync();

		// Group and filter out invalid URLs
		var result = new Dictionary<TeamIdentifier, IEnumerable<SocialAccount>>();
		foreach (var group in accounts.GroupBy(a => a.TeamId))
		{
			var groupAccounts = group.Select(a => new Models.Data.SocialAccount { Url = a.Url, AccountType = a.AccountType }).ToList();
			result[new TeamIdentifier(group.Key)] = FilterValidSocialAccounts(groupAccounts);
		}
		return result;
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

		// Return saved accounts, filtering out any invalid URLs for safety
		var savedAccounts = await this.dbContext.SocialAccounts
			.Where(sa => sa.OwnableType == "NationalGoverningBody" && sa.OwnableId == ngbId)
			.ToListAsync();

		return FilterValidSocialAccounts(savedAccounts);
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

		// Return saved accounts, filtering out any invalid URLs for safety
		var savedAccounts = await this.dbContext.SocialAccounts
			.Where(sa => sa.OwnableType == "Team" && sa.OwnableId == teamId.Id)
			.ToListAsync();

		return FilterValidSocialAccounts(savedAccounts);
	}

	/// <summary>
	/// Filters a collection of social account data entities to only include those with valid URIs.
	/// Invalid URLs are silently skipped to prevent crashes when reading from the database.
	/// </summary>
	private static IEnumerable<SocialAccount> FilterValidSocialAccounts(IEnumerable<Models.Data.SocialAccount> accounts)
	{
		var validAccounts = new List<SocialAccount>();
		foreach (var sa in accounts)
		{
			if (Uri.TryCreate(sa.Url, UriKind.Absolute, out var uri))
			{
				validAccounts.Add(new SocialAccount(uri, sa.AccountType));
			}
		}
		return validAccounts;
	}
}
