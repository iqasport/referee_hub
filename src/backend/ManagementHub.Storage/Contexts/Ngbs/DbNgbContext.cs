using System.Linq;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Ngb;
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

	public IQueryable<INgbContext> QueryNgbs()
	{
		return this.dbContext.NationalGoverningBodies.AsNoTracking()
			.Select(n => new DbNgbContext(
				NgbIdentifier.Parse(n.CountryCode),
				new NgbData
				{
					Name = n.Name,
					Acronym = n.Acronym,
					Country = n.Country,
					MembershipStatus = n.MembershipStatus,
					Region = n.Region,
				}));
	}
}
