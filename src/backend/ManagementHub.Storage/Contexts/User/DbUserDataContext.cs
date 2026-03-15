using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.User;

using User = ManagementHub.Models.Data.User;

public record class DbUserDataContext(UserIdentifier UserId, ExtendedUserData ExtendedUserData) : IUserDataContext
{
}

public class DbUserDataContextFactory
{
	private readonly IQueryable<User> users;
	private readonly IQueryable<Language> languages;
	private readonly ILogger<DbUserDataContextFactory> logger;

	public DbUserDataContextFactory(
		IQueryable<User> users,
		IQueryable<Language> languages,
		ILogger<DbUserDataContextFactory> logger)
	{
		this.users = users;
		this.languages = languages;
		this.logger = logger;
	}

	public async Task<DbUserDataContext> LoadAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(-0x23686b00, "Loading user data context for user ({userId}).", userId);
		var userData = await QueryUserData(this.users.AsNoTracking().WithIdentifier(userId)).SingleOrDefaultAsync(cancellationToken);

		if (userData == null)
		{
			throw new NotFoundException(userId.ToString());
		}

		this.logger.LogInformation(-0x23686aff, "Returning user data context.");

		return new DbUserDataContext(userId, userData);
	}

	internal static IQueryable<ExtendedUserData> QueryUserData(IQueryable<User> users)
	{
		return users
			.Include(u => u.Language)
			.Select(user => new ExtendedUserData(new Email(user.Email), user.FirstName ?? string.Empty, user.LastName ?? string.Empty)
			{
				Bio = user.Bio ?? string.Empty,
				ExportName = user.ExportName ?? true,
				Pronouns = user.Pronouns ?? string.Empty,
				ShowPronouns = user.ShowPronouns ?? false,
				UserLang = user.Language != null ? new LanguageIdentifier(user.Language.ShortName, user.Language.ShortRegion) : LanguageIdentifier.Default,
				CreatedAt = DateOnly.FromDateTime(user.CreatedAt),
			});
	}
}
