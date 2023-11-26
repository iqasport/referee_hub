// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Hangfire;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ManagementHub.Service.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
	private readonly UserManager<UserIdentity> userManager;
	private readonly IBackgroundJobClient backgroundJob;
	private readonly ILogger<ForgotPasswordModel> logger;

	public ForgotPasswordModel(UserManager<UserIdentity> userManager, IBackgroundJobClient backgroundJob, ILogger<ForgotPasswordModel> logger)
	{
		this.userManager = userManager;
		this.backgroundJob = backgroundJob;
		this.logger = logger;
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
	public class InputModel
	{
		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if (this.ModelState.IsValid)
		{
			var user = await this.userManager.FindByEmailAsync(this.Input.Email);
			if (user == null || !await this.userManager.IsEmailConfirmedAsync(user))
			{
				this.logger.LogInformation(-0x2c526c00, "User not found or their email hasn't been confirmed. Skipping sending password reset.");
				// Don't reveal that the user does not exist or is not confirmed
				return this.RedirectToPage("./ForgotPasswordConfirmation");
			}

			// For more information on how to enable account confirmation and password reset please
			// visit https://go.microsoft.com/fwlink/?LinkID=532713
			var code = await this.userManager.GeneratePasswordResetTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			var callbackUrl = this.Url.Page(
				"/Account/ResetPassword",
				pageHandler: null,
				values: new { area = "Identity", code },
				protocol: this.Request.Scheme);

			var userId = user.UserId;
			this.backgroundJob.Enqueue<ISendAccountEmail>(this.logger, sender =>
				sender.SendAccountEmailAsync(userId, "Reset Password - IQA Management Hub",
				$"""
					<p>We've heard you forgot your password. You can reset your password by clicking the link below:</p>
					<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Reset my password</a></p>
					""", CancellationToken.None));

			return this.RedirectToPage("./ForgotPasswordConfirmation");
		}

		return this.Page();
	}
}
