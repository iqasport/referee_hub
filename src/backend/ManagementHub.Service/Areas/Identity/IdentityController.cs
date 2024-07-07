using ManagementHub.Models.Abstraction.Commands.Migrations;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Contexts;
using ManagementHub.Storage.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Identity;

/// <summary>
/// Actions related to exporting users with the referee role.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class IdentityController : ControllerBase
{
	private readonly SignInManager<UserIdentity> signInManager;
	private readonly IUserIdMigrationCommand userIdMigrationCommand;
	private readonly ILogger logger;
	private readonly IUserIdentityRepository userIdentityRepository;
	private readonly ICurrentUserGetter currentUserGetter;

	public IdentityController(SignInManager<UserIdentity> signInManager, IUserIdMigrationCommand userIdMigrationCommand, ILogger<IdentityController> logger, IUserIdentityRepository userIdentityRepository, ICurrentUserGetter currentUserGetter)
	{
		this.signInManager = signInManager;
		this.userIdMigrationCommand = userIdMigrationCommand;
		this.logger = logger;
		this.userIdentityRepository = userIdentityRepository;
		this.currentUserGetter = currentUserGetter;
	}

	/// <summary>
	/// Perform a sign-in action with user credentials to obtain a Bearer token for header based authentication.
	/// </summary>
	[HttpPost("login")]
	[Tags("Identity")]
	[ProducesResponseType(200, Type = typeof(AccessTokenResponse))]
	public async Task LoginAsync([FromBody] LoginRequest input)
	{
		this.signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

		if (this.ModelState.IsValid)
		{
			// The actions in this method should correspond with actions in Identity/Pages/Account/Login
			await this.userIdMigrationCommand.TryMigrateUserIdAsync(input.Email, this.HttpContext.RequestAborted);

			var result = await this.signInManager.PasswordSignInAsync(input.Email, input.Password, isPersistent: false, lockoutOnFailure: false);
			if (result.Succeeded)
			{
				await this.userIdentityRepository.SetLastLoginTime(new UserIdentity(this.currentUserGetter.CurrentUser, new Email(input.Email)), default);

				// This method returns a Task, because SignInManager is actually writing the contents of the HTTP response.
				return;
			}

			if (result.RequiresTwoFactor)
			{
				if (!string.IsNullOrEmpty(input.TwoFactorCode))
				{
					result = await this.signInManager.TwoFactorAuthenticatorSignInAsync(input.TwoFactorCode, isPersistent: false, rememberClient: false);
				}
				else if (!string.IsNullOrEmpty(input.TwoFactorRecoveryCode))
				{
					result = await this.signInManager.TwoFactorRecoveryCodeSignInAsync(input.TwoFactorRecoveryCode);
				}

				if (result.Succeeded)
				{
					return;
				}
			}
		}

		throw new InvalidOperationException();
	}
}
