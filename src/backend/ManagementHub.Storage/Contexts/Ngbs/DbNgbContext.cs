using System;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Ngbs;
public record DbNgbContext(NgbIdentifier NgbId, NgbData NgbData) : INgbContext;

public class DbNgbContextFactory
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<DbNgbContextFactory> logger;

	public DbNgbContextFactory(ManagementHubDbContext dbContext, ILogger<DbNgbContextFactory> logger)
	{
		this.dbContext = dbContext;
		this.logger = logger;
	}

	public IQueryable<INgbContext> QueryNgbs() => this.QueryNgbs(this.dbContext.NationalGoverningBodies);
	public async Task<INgbContext> GetSingleNgb(NgbIdentifier ngb) => await this.QueryNgbs(this.dbContext.NationalGoverningBodies.WithIdentifier(ngb)).SingleAsync();
	private IQueryable<INgbContext> QueryNgbs(IQueryable<NationalGoverningBody> ngbs)
	{
		return ngbs.AsNoTracking()
			.Select(n => new DbNgbContext(
				NgbIdentifier.Parse(n.CountryCode),
				new NgbData
				{
					Name = n.Name,
					Acronym = n.Acronym,
					Country = n.Country,
					MembershipStatus = n.MembershipStatus,
					Region = n.Region,
					Website = n.Website != null ? new Uri(n.Website) : null,
				}));
	}
}
