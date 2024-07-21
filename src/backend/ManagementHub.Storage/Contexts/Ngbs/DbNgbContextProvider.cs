using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Storage.Attachments;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Contexts.User;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Ngbs;
public class DbNgbContextProvider : INgbContextProvider
{
	private readonly DbNgbContextFactory ngbContextFactory;
	private readonly DbNgbStatsContextFactory ngbStatsContextFactory;
	private readonly DbNgbAvatarContextFactory dbNgbAvatarContextFactory;

	public DbNgbContextProvider(
		ManagementHubDbContext dbContext,
		IAttachmentRepository attachmentRepository,
		IAccessFileCommand accessFile,
		CollectionFilteringContext filteringContext,
		ILoggerFactory loggerFactory)
	{
		this.ngbContextFactory = new(dbContext, loggerFactory.CreateLogger<DbNgbContextFactory>(), filteringContext);
		this.ngbStatsContextFactory = new(dbContext);
		this.dbNgbAvatarContextFactory = new(attachmentRepository, accessFile, loggerFactory.CreateLogger<DbNgbAvatarContextFactory>());
	}

	public async Task<IOrderedEnumerable<INgbStatsContext>> GetHistoricalNgbStatsAsync(NgbIdentifier ngb)
	{
		return await this.ngbStatsContextFactory.GetHistoricalNgbStatsAsync(ngb);
	}

	public Task<Uri?> GetNgbAvatarUriAsync(NgbIdentifier ngb)
	{
		return this.dbNgbAvatarContextFactory.GetNgbAvatarUriAsync(ngb, default);
	}

	public async Task<INgbContext> GetNgbContextAsync(NgbIdentifier ngb)
	{
		return await this.ngbContextFactory.GetSingleNgb(ngb);
	}

	public async Task<INgbStatsContext> GetCurrentNgbStatsAsync(NgbIdentifier ngb)
	{
		return await this.ngbStatsContextFactory.GetNgbStatsContextAsync(ngb);
	}

	public IQueryable<INgbContext> QueryNgbs()
	{
		return this.ngbContextFactory.QueryNgbs();
	}

	public IAsyncEnumerable<INgbContext> GetNgbContextsAsync(NgbConstraint ngb)
	{
		return this.ngbContextFactory.GetMultipleNgbs(ngb);
	}

	public Task UpdateNgbInfoAsync(NgbIdentifier ngb, NgbData ngbData)
	{
		return this.ngbContextFactory.UpdateNgb(ngb, ngbData);
	}

	public Task CreateNgbAsync(NgbIdentifier ngb, NgbData ngbData)
	{
		return this.ngbContextFactory.CreateNgb(ngb, ngbData);
	}
}
