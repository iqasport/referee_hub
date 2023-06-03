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
		this.logger.LogInformation(0, "Loading user data context for user ({userId}).", userId);
		var userData = await this.users.WithIdentifier(userId)
			// THIS LEFT JOIN IN PURE LINQ
			.GroupJoin(this.languages, u => u.LanguageId, l => l.Id, (u, l) => new { User = u, Languages = l })
			.SelectMany(join => join.Languages.DefaultIfEmpty(), (join, l) => new { join.User, Language = l })
			// UNTIL HERE
			.Select(join => new ExtendedUserData(new Email(join.User.Email), join.User.FirstName ?? string.Empty, join.User.LastName ?? string.Empty)
			{
				Bio = join.User.Bio ?? string.Empty,
				ExportName = join.User.ExportName ?? true,
				Pronouns = join.User.Pronouns ?? string.Empty,
				ShowPronouns = join.User.ShowPronouns ?? false,
				UserLang = join.Language != null ? new LanguageIdentifier(join.Language.ShortName, join.Language.ShortRegion) : LanguageIdentifier.Default,
			})
			.SingleOrDefaultAsync(cancellationToken);

		if (userData == null)
		{
			throw new NotFoundException(userId.ToString());
		}

		this.logger.LogInformation(0, "Returning user data context.");

		return new DbUserDataContext(userId, userData);
	}
}
