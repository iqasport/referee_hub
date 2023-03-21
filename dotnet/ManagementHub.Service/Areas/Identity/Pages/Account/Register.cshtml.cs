// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Database.Transactions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Service.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
	private readonly SignInManager<UserIdentity> signInManager;
	private readonly UserManager<UserIdentity> userManager;
	private readonly IUserStore<UserIdentity> userStore;
	private readonly IUserEmailStore<UserIdentity> emailStore;
	private readonly ILogger<RegisterModel> logger;
	private readonly IEmailSender emailSender;
	private readonly IUpdateUserDataCommand updateUserDataCommand;
	private readonly IDatabaseTransactionProvider databaseTransactionProvider;

	public RegisterModel(
		UserManager<UserIdentity> userManager,
		IUserStore<UserIdentity> userStore,
		SignInManager<UserIdentity> signInManager,
		ILogger<RegisterModel> logger,
		IEmailSender emailSender,
		IUpdateUserDataCommand updateUserDataCommand,
		IDatabaseTransactionProvider databaseTransactionProvider)
	{
		this.userManager = userManager;
		this.userStore = userStore;
		this.emailStore = userStore as IUserEmailStore<UserIdentity>;
		this.signInManager = signInManager;
		this.logger = logger;
		this.emailSender = emailSender;
		this.updateUserDataCommand = updateUserDataCommand;
		this.databaseTransactionProvider = databaseTransactionProvider;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[BindProperty]
	public InputModel Input { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string ReturnUrl { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public IList<AuthenticationScheme> ExternalLogins { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public class InputModel
	{
		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
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

			await using var transaction = await this.databaseTransactionProvider.BeginAsync();

			var result = await this.userManager.CreateAsync(user, this.Input.Password);

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					this.ModelState.AddModelError(string.Empty, error.Description);
				}

				return this.Page();
			}

			this.logger.LogInformation("User created a new account with password.");

			// TODO: pass name and surname from registration form
			await this.updateUserDataCommand.UpdateUserDataAsync(user.UserId, (data) => new ExtendedUserData(data.Email, "John", "Smith"), default);

			// TODO: refactor this code to make it readable
			await transaction.CommitAsync();

			var userId = await this.userManager.GetUserIdAsync(user);
			var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			var callbackUrl = this.Url.Page(
				"/Account/ConfirmEmail",
				pageHandler: null,
				values: new { area = "Identity", userId, code, returnUrl },
				protocol: this.Request.Scheme);

			await this.emailSender.SendEmailAsync(this.Input.Email, "Confirm your email",
				$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

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

}
