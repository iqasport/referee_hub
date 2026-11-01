using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Contexts.User;
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
		this.logger.LogInformation(0xe860100, "Performing update on user ({userId})", userId);

		await using var transaction = await this.transactionProvider.BeginAsync();

		var userData = await DbUserDataContextFactory.QueryUserData(this.users.AsNoTracking().WithIdentifier(userId))
			.SingleAsync(cancellationToken);

		var newUserData = updater(userData);

		const int numberOfPropertiesOfExtendedUserData = 9;
		var propertySetters = new List<Expr>(capacity: numberOfPropertiesOfExtendedUserData);
		var propertyNames = new List<string>(capacity: numberOfPropertiesOfExtendedUserData);

		var namesChanged = !string.Equals(newUserData.FirstName, userData.FirstName, StringComparison.InvariantCulture)
			|| !string.Equals(newUserData.LastName, userData.LastName, StringComparison.InvariantCulture);
		var requestedUsername = newUserData.Username == null ? null : UsernameRules.Normalize(newUserData.Username);
		if (requestedUsername != null && !string.Equals(requestedUsername, userData.Username, StringComparison.Ordinal))
		{
			if (!UsernameRules.IsValid(requestedUsername))
			{
				throw new ArgumentException($"'{requestedUsername}' is not a valid username.");
			}

			var isTaken = await this.users.AsNoTracking()
				.Where(u => u.Username == requestedUsername)
				.AnyAsync(cancellationToken);
			if (isTaken)
			{
				throw new UsernameTakenException(requestedUsername);
			}

			propertyNames.Add(nameof(newUserData.Username));
			propertySetters.Add(s => s.SetProperty(u => u.Username, requestedUsername));
		}
		else if (namesChanged || userData.Username == null)
		{
			// keep the auto-generated default username in sync with the user's names until they pick their own
			var userDbId = await this.users.AsNoTracking().WithIdentifier(userId).Select(u => u.Id).SingleAsync(cancellationToken);
			if (userData.Username == null || userData.Username == UsernameRules.FallbackFor(userDbId))
			{
				var generated = await this.users.GenerateDefaultUsernameAsync(userDbId, newUserData.FirstName, newUserData.LastName, cancellationToken);
				if (!string.Equals(generated, userData.Username, StringComparison.Ordinal))
				{
					propertyNames.Add(nameof(newUserData.Username));
					propertySetters.Add(s => s.SetProperty(u => u.Username, generated));
				}
			}
		}

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
			this.logger.LogInformation(0xe860101, "Updating user data for ({userId}) on properties: {propertyNames}.", userId, string.Join(", ", propertyNames));

			int result;
			try
			{
				result = await this.users.AsNoTracking().WithIdentifier(userId).ExecuteUpdateAsync(propertySetters, cancellationToken);
			}
			catch (DbException ex) when (ex.SqlState == "23505" && propertyNames.Contains(nameof(newUserData.Username)))
			{
				// unique index violation - another user claimed the username concurrently
				throw new UsernameTakenException(requestedUsername ?? string.Empty);
			}

			Debug.Assert(result == 1); // only one row of the specific user should have been updated.

			await transaction.CommitAsync(cancellationToken);
		}
		else
		{
			this.logger.LogInformation(0xe860102, "No changes have been made to the user.");
		}
	}
}
