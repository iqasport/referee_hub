using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Expr = System.Linq.Expressions.Expression<System.Func<Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<ManagementHub.Models.Data.User>, Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<ManagementHub.Models.Data.User>>>;

namespace ManagementHub.Storage.Commands.User;
using User = ManagementHub.Models.Data.User;

public class UpdateUserDataCommand : IUpdateUserDataCommand
{
	private readonly IQueryable<User> users;
	private readonly IQueryable<Language> languages;
	private readonly ILogger<UpdateUserDataCommand> logger;
	private readonly ISystemClock clock;
	private readonly IDatabaseTransactionProvider transactionProvider;

	public UpdateUserDataCommand(
		IQueryable<User> users,
		IQueryable<Language> languages,
		ILogger<UpdateUserDataCommand> logger,
		ISystemClock clock,
		IDatabaseTransactionProvider transactionProvider)
	{
		this.users = users;
		this.languages = languages;
		this.logger = logger;
		this.clock = clock;
		this.transactionProvider = transactionProvider;
	}

	public async Task UpdateUserDataAsync(UserIdentifier userId, Func<ExtendedUserData, ExtendedUserData> updater, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0, "Performing update on user ({userId})", userId);

		await using var transaction = await this.transactionProvider.BeginAsync();

		//TODO: this was copied from DbUserDataContext and should be refactored into some single place
		var userData = await this.users.AsNoTracking().WithIdentifier(userId)
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
			.SingleAsync(cancellationToken);

		var newUserData = updater(userData);

		const int numberOfPropertiesOfExtendedUserData = 8;
		var propertySetters = new List<Expr>(capacity: numberOfPropertiesOfExtendedUserData);
		var propertyNames = new List<string>(capacity: numberOfPropertiesOfExtendedUserData);

		if (!string.Equals(newUserData.FirstName, userData.FirstName, StringComparison.InvariantCulture))
		{
			propertyNames.Add(nameof(newUserData.FirstName));
			var value = newUserData.FirstName;
			propertySetters.Add(s => s.SetProperty(u => u.FirstName, value));
		}

		if (!string.Equals(newUserData.LastName, userData.LastName, StringComparison.InvariantCulture))
		{
			propertyNames.Add(nameof(newUserData.LastName));
			var value = newUserData.LastName;
			propertySetters.Add(s => s.SetProperty(u => u.LastName, value));
		}

		if (newUserData.Email != userData.Email)
		{
			propertyNames.Add(nameof(newUserData.Email));
			var value = newUserData.Email.Value;
			propertySetters.Add(s => s.SetProperty(u => u.Email, value));
		}

		if (!string.Equals(newUserData.Bio, userData.Bio, StringComparison.InvariantCulture))
		{
			propertyNames.Add(nameof(newUserData.Bio));
			var value = newUserData.Bio;
			propertySetters.Add(s => s.SetProperty(u => u.Bio, value));
		}

		if (!string.Equals(newUserData.Pronouns, userData.Pronouns, StringComparison.InvariantCulture))
		{
			propertyNames.Add(nameof(newUserData.Pronouns));
			var value = newUserData.Pronouns;
			propertySetters.Add(s => s.SetProperty(u => u.Pronouns, value));
		}

		if (newUserData.ShowPronouns != userData.ShowPronouns)
		{
			propertyNames.Add(nameof(newUserData.ShowPronouns));
			var value = newUserData.ShowPronouns;
			propertySetters.Add(s => s.SetProperty(u => u.ShowPronouns, value));
		}

		if (newUserData.ExportName != userData.ExportName)
		{
			propertyNames.Add(nameof(newUserData.ExportName));
			var value = newUserData.ExportName;
			propertySetters.Add(s => s.SetProperty(u => u.ExportName, value));
		}

		if (newUserData.UserLang != userData.UserLang)
		{
			propertyNames.Add(nameof(newUserData.UserLang));
			var newLangId = await this.languages.AsNoTracking()
				.Where(l => l.ShortName == newUserData.UserLang.Lang && l.ShortRegion == newUserData.UserLang.Region)
				.Select(l => l.Id)
				.SingleAsync(cancellationToken);
			propertySetters.Add(s => s.SetProperty(u => u.LanguageId, newLangId));
		}

		// if any property has been changed we also want to update the last update time
		if (propertySetters.Count > 0)
		{
			var now = this.clock.UtcNow.UtcDateTime;
			propertySetters.Add(s => s.SetProperty(u => u.UpdatedAt, now));
		}

		if (propertySetters.Count > 0)
		{
			this.logger.LogInformation(0, "Updating user data for ({userId}) on properties: {propertyNames}.", userId, string.Join(", ", propertyNames));

			var result = await this.users.AsNoTracking().WithIdentifier(userId).ExecuteUpdateAsync(propertySetters, cancellationToken);
			Debug.Assert(result == 1); // only one row of the specific user should have been updated.

			await transaction.CommitAsync(cancellationToken);
		}
		else
		{
			this.logger.LogInformation(0, "No changes have been made to the user.");
		}
	}
}
