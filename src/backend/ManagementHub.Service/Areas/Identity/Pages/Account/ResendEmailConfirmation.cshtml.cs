// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Hangfire;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ManagementHub.Service.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ResendEmailConfirmationModel : PageModel
{
	private readonly UserManager<UserIdentity> userManager;
	private readonly IBackgroundJobClient backgroundJob;
	private readonly ILogger<ResendEmailConfirmationModel> logger;

	public ResendEmailConfirmationModel(UserManager<UserIdentity> userManager, IBackgroundJobClient backgroundJob, ILogger<ResendEmailConfirmationModel> logger)
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

	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if (!this.ModelState.IsValid)
		{
			return this.Page();
		}

		var user = await this.userManager.FindByEmailAsync(this.Input.Email);
		if (user == null)
		{
			this.ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
			return this.Page();
		}

		var userId = await this.userManager.GetUserIdAsync(user);
		var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
		var callbackUrl = this.Url.Page(
			"/Account/ConfirmEmail",
			pageHandler: null,
			values: new { userId, code },
			protocol: this.Request.Scheme);

		var userIdentifier = user.UserId;
		this.backgroundJob.Enqueue<ISendAccountEmail>(this.logger, sender =>
			sender.SendAccountEmailAsync(userIdentifier, "Confirm your email - IQA Management Hub",
			$"""
				<p>Please confirm your account by clicking the link below:</p>
				<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Confirm email address</a></p>
				""", CancellationToken.None));

		this.ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
		return this.Page();
	}
}
