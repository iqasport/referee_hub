// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
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
public class RegisterConfirmationModel : PageModel
{
	private readonly UserManager<UserIdentity> userManager;
	private readonly IBackgroundJobClient backgroundJob;
	private readonly ILogger<RegisterConfirmationModel> logger;

	public RegisterConfirmationModel(UserManager<UserIdentity> userManager, IBackgroundJobClient backgroundJob, ILogger<RegisterConfirmationModel> logger)
	{
		this.userManager = userManager;
		this.backgroundJob = backgroundJob;
		this.logger = logger;
	}

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public bool DisplayConfirmAccountLink { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string EmailConfirmationUrl { get; set; }

	public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
	{
		if (email == null)
		{
			return this.RedirectToPage("/Index");
		}
		returnUrl ??= this.Url.Content("~/");

		var user = await this.userManager.FindByEmailAsync(email);
		if (user == null)
		{
			return this.NotFound($"Unable to load user with email '{email}'.");
		}

		var userId = await this.userManager.GetUserIdAsync(user);
		var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
		var callbackUrl = this.EmailConfirmationUrl = this.Url.Page(
			"/Account/ConfirmEmail",
			pageHandler: null,
			values: new { area = "Identity", userId, code, returnUrl },
			protocol: this.Request.Scheme);

		var userIdentifier = user.UserId;
		this.backgroundJob.Enqueue<ISendAccountEmail>(this.logger, sender =>
			sender.SendAccountEmailAsync(userIdentifier, "Confirm your email - IQA Management Hub",
			$"""
			<p>Please confirm your account by clicking the link below:</p>
			<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Confirm email address</a></p>
			""", CancellationToken.None));

		return this.Page();
	}
}
