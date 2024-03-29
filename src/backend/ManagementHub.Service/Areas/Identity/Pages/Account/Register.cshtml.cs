// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Hangfire;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Extensions;
using ManagementHub.Storage.Database.Transactions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ManagementHub.Service.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
	private readonly SignInManager<UserIdentity> signInManager;
	private readonly UserManager<UserIdentity> userManager;
	private readonly IUserStore<UserIdentity> userStore;
	private readonly IUserEmailStore<UserIdentity> emailStore;
	private readonly ILogger<RegisterModel> logger;
	private readonly IUpdateUserDataCommand updateUserDataCommand;
	private readonly IDatabaseTransactionProvider databaseTransactionProvider;
	private readonly IBackgroundJobClient backgroundJob;

	public RegisterModel(
		UserManager<UserIdentity> userManager,
		IUserStore<UserIdentity> userStore,
		SignInManager<UserIdentity> signInManager,
		ILogger<RegisterModel> logger,
		IUpdateUserDataCommand updateUserDataCommand,
		IDatabaseTransactionProvider databaseTransactionProvider,
		IBackgroundJobClient backgroundJob)
	{
		this.userManager = userManager;
		this.userStore = userStore;
		this.emailStore = userStore as IUserEmailStore<UserIdentity>;
		this.signInManager = signInManager;
		this.logger = logger;
		this.updateUserDataCommand = updateUserDataCommand;
		this.databaseTransactionProvider = databaseTransactionProvider;
		this.backgroundJob = backgroundJob;
	}

	[BindProperty]
	public InputModel Input { get; set; }

	public string ReturnUrl { get; set; }

	public IList<AuthenticationScheme> ExternalLogins { get; set; }

	public class InputModel
	{
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required]
		[StringLength(1024, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[Required]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Display(Name = "Last Name")]
		public string LastName { get; set; }
	}


	public async Task OnGetAsync(string returnUrl = null)
	{
		this.ReturnUrl = returnUrl;
		this.ExternalLogins = (await this.signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
	}

	public async Task<IActionResult> OnPostAsync(string returnUrl = null)
	{
		returnUrl ??= this.Url.Content("~/");
		this.ExternalLogins = (await this.signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
		if (this.ModelState.IsValid)
		{
			var user = new UserIdentity(UserIdentifier.NewUserId(), new Email(this.Input.Email));

			var result = await this.CreateUser(user);
			if (!result.Succeeded)
			{
				this.AddIdentityErrorsToModel(result);

				return this.Page();
			}

			await this.SendConfirmationEmail(user);

			if (this.userManager.Options.SignIn.RequireConfirmedAccount)
			{
				return this.RedirectToPage("RegisterConfirmation", new { email = this.Input.Email, returnUrl });
			}
			else
			{
				await this.signInManager.SignInAsync(user, isPersistent: false);
				return this.LocalRedirect(returnUrl);
			}
		}

		// If we got this far, something failed, redisplay form
		return this.Page();
	}

	private async Task SendConfirmationEmail(UserIdentity user)
	{
		var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
		var userId = user.UserId;
		var callbackUrl = this.Url.Page(
			"/Account/ConfirmEmail",
			pageHandler: null,
			values: new { area = "Identity", userId = userId.ToString(), code },
			protocol: this.Request.Scheme);

		this.backgroundJob.Enqueue<ISendAccountEmail>(this.logger, sender =>
			sender.SendAccountEmailAsync(userId, "Confirm your email - IQA Management Hub",
			$"""
				<p>Welcome to the Management Hub {this.Input.FirstName}!</p>
				<p>You can confirm your account email through the link below:</p>
				<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Confirm my account</a></p>
				""", CancellationToken.None));
	}

	private void AddIdentityErrorsToModel(IdentityResult result)
	{
		foreach (var error in result.Errors)
		{
			this.ModelState.AddModelError(string.Empty, error.Description);
		}
	}

	private async Task<IdentityResult> CreateUser(UserIdentity user)
	{
		try
		{
			await using (var transaction = await this.databaseTransactionProvider.BeginAsync())
			{
				var result = await this.userManager.CreateAsync(user, this.Input.Password);

				if (!result.Succeeded)
				{
					return result;
				}

				this.logger.LogInformation("User created a new account with password.");

				await this.updateUserDataCommand.UpdateUserDataAsync(user.UserId, (data) => new ExtendedUserData(data.Email, this.Input.FirstName, this.Input.LastName), default);

				await transaction.CommitAsync();
			}

			return IdentityResult.Success;
		}
		catch (Exception ex)
		{
			this.logger.LogError(-0x2ac59700, ex, "Error while creating the user.");
			return IdentityResult.Failed(new IdentityError { Description = ex.Message });
		}
	}
}
