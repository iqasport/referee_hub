using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Services;
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

internal sealed record StoredUserData(
	string Email,
	string FirstName,
	string LastName,
	string? Bio,
	bool? ExportName,
	string? Pronouns,
	bool? ShowPronouns,
	LanguageIdentifier UserLang,
	DateOnly CreatedAt);

public class DbUserDataContextFactory
{
	private readonly IQueryable<User> users;
	private readonly IQueryable<Language> languages;
	private readonly IUserSensitiveInfoProtector sensitiveInfoProtector;
	private readonly ILogger<DbUserDataContextFactory> logger;

	public DbUserDataContextFactory(
		IQueryable<User> users,
		IQueryable<Language> languages,
		IUserSensitiveInfoProtector sensitiveInfoProtector,
		ILogger<DbUserDataContextFactory> logger)
	{
		this.users = users;
		this.languages = languages;
		this.sensitiveInfoProtector = sensitiveInfoProtector;
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

		return new DbUserDataContext(userId, ToExtendedUserData(userData, this.sensitiveInfoProtector));
	}

	internal static IQueryable<StoredUserData> QueryUserData(IQueryable<User> users)
	{
		return users
			.Include(u => u.Language)
			.Select(user => new StoredUserData(
				user.Email,
				user.FirstName ?? string.Empty,
				user.LastName ?? string.Empty,
				user.Bio,
				user.ExportName,
				user.Pronouns,
				user.ShowPronouns,
				user.Language != null ? new LanguageIdentifier(user.Language.ShortName, user.Language.ShortRegion) : LanguageIdentifier.Default,
				DateOnly.FromDateTime(user.CreatedAt)));
	}

	internal static ExtendedUserData ToExtendedUserData(StoredUserData userData, IUserSensitiveInfoProtector sensitiveInfoProtector)
	{
		return new ExtendedUserData(new Email(userData.Email), userData.FirstName, userData.LastName)
		{
			Bio = sensitiveInfoProtector.Unprotect(userData.Bio) ?? string.Empty,
			ExportName = userData.ExportName ?? true,
			Pronouns = sensitiveInfoProtector.Unprotect(userData.Pronouns) ?? string.Empty,
			ShowPronouns = userData.ShowPronouns ?? false,
			UserLang = userData.UserLang,
			CreatedAt = userData.CreatedAt,
		};
	}
}
