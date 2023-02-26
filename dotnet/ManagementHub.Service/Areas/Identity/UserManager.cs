using ManagementHub.Models.Domain.User;
using Microsoft.AspNetCore.Identity;
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
}
