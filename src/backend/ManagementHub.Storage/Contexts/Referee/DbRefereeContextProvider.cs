﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Referee;
public class DbRefereeContextProvider : IRefereeContextProvider
{
	private readonly DbRefereeViewContextFactory dbRefereeViewContextFactory;
	private readonly DbRefereeTestContextFactory dbRefereeTestContextFactory;
	private readonly DbRefereeEmailFeedbackContextFactory dbRefereeEmailFeedbackContextFactory;

	public DbRefereeContextProvider(
		ManagementHubDbContext dbContext,
		ITestContextProvider testContextProvider,
		CollectionFilteringContext filteringContext,
		ILoggerFactory loggerFactory)
	{
		this.dbRefereeViewContextFactory = new DbRefereeViewContextFactory(
			dbContext,
			loggerFactory.CreateLogger<DbRefereeViewContextFactory>(),
			filteringContext);

		this.dbRefereeTestContextFactory = new DbRefereeTestContextFactory(
			dbContext.Users,
			loggerFactory.CreateLogger<DbRefereeTestContextFactory>());

		this.dbRefereeEmailFeedbackContextFactory = new DbRefereeEmailFeedbackContextFactory(
			dbContext.TestAttempts,
			dbContext.TestResults,
			testContextProvider,
			loggerFactory.CreateLogger<DbRefereeEmailFeedbackContextFactory>());
	}

	public async Task<IRefereeEmailFeedbackContext> GetRefereeEmailFeedbackContextAsync(TestAttemptIdentifier testAttemptId, CancellationToken cancellationToken)
	{
		return await this.dbRefereeEmailFeedbackContextFactory.LoadAsync(testAttemptId, cancellationToken);
	}

	public async Task<IRefereeTestContext> GetRefereeTestContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		return await this.dbRefereeTestContextFactory.LoadAsync(userId, cancellationToken);
	}

	public async Task<IRefereeViewContext> GetRefereeViewContextAsync(UserIdentifier userId, NgbConstraint ngbConstraint, CancellationToken cancellationToken)
	{
		return await this.dbRefereeViewContextFactory.LoadAsync(userId, ngbConstraint, cancellationToken);
	}

	public IAsyncEnumerable<IRefereeViewContext> GetRefereeViewContextAsyncEnumerable(NgbConstraint ngbConstraint)
	{
		return this.GetRefereeViewContextQueryable(ngbConstraint).AsAsyncEnumerable();
	}

	public IQueryable<IRefereeViewContext> GetRefereeViewContextQueryable(NgbConstraint ngbConstraint)
	{
		return this.dbRefereeViewContextFactory.QueryReferees(ngbConstraint);
	}
}
