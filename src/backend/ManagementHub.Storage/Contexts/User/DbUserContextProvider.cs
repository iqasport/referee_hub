﻿using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Attachments;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.User;

/// <summary>
/// Implementation of <see cref="IUserContextProvider"/> that loads data from the database using factory classes.
/// </summary>
public class DbUserContextProvider : IUserContextProvider
{
	private readonly DbUserContextFactory userContextFactory;
	private readonly DbUserDataContextFactory userDataContextFactory;
	private readonly DbUserAvatarContextFactory userAvatarContextFactory;

	public DbUserContextProvider(
		ManagementHubDbContext dbContext,
		IAttachmentRepository attachmentRepository,
		IAccessFileCommand accessFile,
		ILoggerFactory loggerFactory)
	{
		this.userContextFactory = new DbUserContextFactory(
			dbContext.Users,
			dbContext.Roles,
			dbContext.NationalGoverningBodyAdmins,
			dbContext.RefereeTeams,
			dbContext.RefereeLocations,
			dbContext.Languages,
			loggerFactory.CreateLogger<DbUserContextFactory>());

		this.userDataContextFactory = new DbUserDataContextFactory(
			dbContext.Users,
			dbContext.Languages,
			loggerFactory.CreateLogger<DbUserDataContextFactory>());

		this.userAvatarContextFactory = new DbUserAvatarContextFactory(
			attachmentRepository,
			accessFile,
			loggerFactory.CreateLogger<DbUserAvatarContextFactory>());
	}

	public async Task<UserAttributes> GetUserAttributesAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		return await this.userContextFactory.GetUserAttributesAsync(userId, cancellationToken);
	}

	public async Task<IUserAvatarContext> GetUserAvatarContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		return await this.userAvatarContextFactory.LoadAsync(userId, cancellationToken);
	}

	public async Task<IUserContext> GetUserContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		return await this.userContextFactory.LoadAsync(userId, cancellationToken);
	}

	public async Task<IUserDataContext> GetUserDataContextAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		return await this.userDataContextFactory.LoadAsync(userId, cancellationToken);
	}
}
