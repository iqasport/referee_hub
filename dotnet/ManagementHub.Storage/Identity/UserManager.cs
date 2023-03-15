using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Service.Areas.Identity;

/// <summary>
/// Custom overrides over the user manager from ASP.NET Core Identity
/// </summary>
public class UserManager : UserManager<UserIdentity>
{
    public UserManager(
        IUserStore<UserIdentity> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<UserIdentity> passwordHasher,
        IEnumerable<IUserValidator<UserIdentity>> userValidators,
        IEnumerable<IPasswordValidator<UserIdentity>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<UserIdentity>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    // override user creation to skip an assumption made on email normalization
    public override async Task<IdentityResult> CreateAsync(UserIdentity user)
    {
		this.ThrowIfDisposed();
        var result = await this.ValidateUserAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return result;
        }

        return await this.Store.CreateAsync(user, this.CancellationToken).ConfigureAwait(false);
    }

	// override to not call UpdateAsync()
	public override async Task<IdentityResult> ConfirmEmailAsync(UserIdentity user, string token)
	{
		this.ThrowIfDisposed();
		var store = (IUserEmailStore<UserIdentity>)this.Store;
		if (user == null)
		{
			throw new ArgumentNullException(nameof(user));
		}

		if (!await this.VerifyUserTokenAsync(user, this.Options.Tokens.EmailConfirmationTokenProvider, ConfirmEmailTokenPurpose, token).ConfigureAwait(false))
		{
			return IdentityResult.Failed(this.ErrorDescriber.InvalidToken());
		}

		try
		{
			await store.SetEmailConfirmedAsync(user, true, this.CancellationToken).ConfigureAwait(false);
			return IdentityResult.Success;
		}
		catch (Exception ex) when (ex is not OutOfMemoryException)
		{
			this.Logger.LogError(0, ex, "Failed to confirm email.");
			return IdentityResult.Failed();
		}
	}

	// override to not call UpdateAsync() and to only call SetEmailConfirmed if it wasn't before
	/// <summary>
	/// Changes email address using a token sent to the new address (generated with <see cref="base.GenerateChangeEmailTokenAsync()"/>).
	/// </summary>
	public override async Task<IdentityResult> ChangeEmailAsync(UserIdentity user, string newEmail, string token)
	{
		this.ThrowIfDisposed();
		if (user == null)
		{
			throw new ArgumentNullException(nameof(user));
		}

		// Make sure the token is valid and the stamp matches
		if (!await this.VerifyUserTokenAsync(user, this.Options.Tokens.ChangeEmailTokenProvider, GetChangeEmailTokenPurpose(newEmail), token).ConfigureAwait(false))
		{
			return IdentityResult.Failed(this.ErrorDescriber.InvalidToken());
		}

		var store = (IUserEmailStore<UserIdentity>)this.Store;
		try
		{
			await store.SetEmailAsync(user, newEmail, this.CancellationToken).ConfigureAwait(false);

			// to change the email the user had to provide a change token (sent to their email) so if they haven't confirmed before we can confirm now.
			if (!user.IsEmailConfirmed)
			{
				await store.SetEmailConfirmedAsync(user, true, this.CancellationToken).ConfigureAwait(false);
			}
			
			return IdentityResult.Success;
		}
		catch (Exception ex) when (ex is not OutOfMemoryException)
		{
			this.Logger.LogError(0, ex, "Failed to change email.");
			return IdentityResult.Failed();
		}
	}

	// override to not call UpdateAsync() and to only call SetEmailConfirmed if it wasn't before
	/// <summary>
	/// Changes email address using a token sent to the new address (generated with <see cref="base.GeneratePasswordResetTokenAsync()"/>).
	/// </summary>
	public override async Task<IdentityResult> ResetPasswordAsync(UserIdentity user, string token, string newPassword)
	{
		this.ThrowIfDisposed();
		if (user == null)
		{
			throw new ArgumentNullException(nameof(user));
		}

		// Make sure the token is valid and the stamp matches
		if (!await this.VerifyUserTokenAsync(user, this.Options.Tokens.PasswordResetTokenProvider, ResetPasswordTokenPurpose, token).ConfigureAwait(false))
		{
			return IdentityResult.Failed(this.ErrorDescriber.InvalidToken());
		}
		return await this.UpdatePasswordHash(user, newPassword, validatePassword: true).ConfigureAwait(false);
	}
}
