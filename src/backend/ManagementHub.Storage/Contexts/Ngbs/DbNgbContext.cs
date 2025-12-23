using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Ngbs;

public record DbNgbContext(NgbIdentifier NgbId, NgbData NgbData) : INgbContext;

public class DbNgbContextFactory
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<DbNgbContextFactory> logger;
	private readonly CollectionFilteringContext filteringContext;

	public DbNgbContextFactory(ManagementHubDbContext dbContext, ILogger<DbNgbContextFactory> logger, CollectionFilteringContext filteringContext)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.filteringContext = filteringContext;
	}

	public IQueryable<INgbContext> QueryNgbs()
	{
		var filter = this.filteringContext.FilteringParameters.Filter;
		filter = string.IsNullOrEmpty(filter) ? filter : $"%{filter}%";
		IQueryable<NationalGoverningBody> filteredNgbs;
		if (this.dbContext.Database.IsNpgsql())
		{
			filteredNgbs = string.IsNullOrEmpty(filter)
				? this.dbContext.NationalGoverningBodies
				: this.dbContext.NationalGoverningBodies
					.Where(ngb => EF.Functions.ILike(ngb.Name, filter));
		}
		else
		{
			filteredNgbs = string.IsNullOrEmpty(filter)
				? this.dbContext.NationalGoverningBodies
				: this.dbContext.NationalGoverningBodies
					.Where(ngb => EF.Functions.Like(ngb.Name, filter));
		}

		if (this.filteringContext.FilteringMetadata != null)
		{
			//TODO: figure out some way to make this async in a nice way
			this.filteringContext.FilteringMetadata.TotalCount = filteredNgbs.Count();
		}

		return this.QueryNgbs(filteredNgbs.Page(this.filteringContext.FilteringParameters));
	}
	public async Task<INgbContext> GetSingleNgb(NgbIdentifier ngb) => (await this.QueryNgbs(this.dbContext.NationalGoverningBodies.WithIdentifier(ngb)).SingleOrDefaultAsync()) ?? throw new NotFoundException(ngb.ToString());
	public IAsyncEnumerable<INgbContext> GetMultipleNgbs(NgbConstraint ngb) => this.QueryNgbs(this.dbContext.NationalGoverningBodies.WithConstraint(ngb)).AsAsyncEnumerable();

	public async Task UpdateNgb(NgbIdentifier ngb, NgbData data)
	{
		var ngbEntity = await this.dbContext.NationalGoverningBodies.WithIdentifier(ngb).SingleAsync();
		ngbEntity.Name = data.Name;
		ngbEntity.Acronym = data.Acronym;
		ngbEntity.Country = data.Country;
		ngbEntity.MembershipStatus = data.MembershipStatus;
		ngbEntity.PlayerCount = data.PlayerCount;
		ngbEntity.Region = data.Region;
		ngbEntity.Website = data.Website?.ToString();
		ngbEntity.UpdatedAt = DateTime.UtcNow;
		await this.dbContext.SaveChangesAsync();
	}

	public async Task CreateNgb(NgbIdentifier ngb, NgbData data)
	{
		this.dbContext.NationalGoverningBodies.Add(new NationalGoverningBody
		{
			CountryCode = ngb.ToString(),
			Name = data.Name,
			Acronym = data.Acronym,
			Country = data.Country,
			MembershipStatus = data.MembershipStatus,
			PlayerCount = data.PlayerCount,
			Region = data.Region,
			Website = data.Website?.ToString(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		});
		await this.dbContext.SaveChangesAsync();
	}

	public async Task<IEnumerable<string>> GetNgbAdminEmails(NgbIdentifier ngb)
	{
		return await this.dbContext.NationalGoverningBodyAdmins
			.Where(a => a.NationalGoverningBody.CountryCode == ngb.ToString())
			.Include(a => a.User)
			.Select(a => a.User.Email)
			.ToListAsync();
	}

	private IQueryable<INgbContext> QueryNgbs(IQueryable<NationalGoverningBody> ngbs)
	{
		Uri? uri;
		return ngbs.AsNoTracking()
			.OrderBy(n => n.CountryCode)
			.Select(n => new DbNgbContext(
				NgbIdentifier.Parse(n.CountryCode),
				new NgbData
				{
					Name = n.Name,
					Acronym = n.Acronym,
					Country = n.Country,
					MembershipStatus = n.MembershipStatus,
					PlayerCount = n.PlayerCount,
					Region = n.Region,
					Website = !string.IsNullOrEmpty(n.Website) && Uri.TryCreate(n.Website, UriKind.Absolute, out uri) ? new Uri(n.Website) : null,
				}));
	}
}
