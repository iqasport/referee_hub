// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using ManagementHub.Models.Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ManagementHub.Service.Areas.Identity.Pages.Account;

public class LogoutModel : PageModel
{
	private readonly SignInManager<UserIdentity> signInManager;
	private readonly ILogger<LogoutModel> logger;

	public LogoutModel(SignInManager<UserIdentity> signInManager, ILogger<LogoutModel> logger)
	{
		this.signInManager = signInManager;
		this.logger = logger;
	}

	public async Task<IActionResult> OnPost(string returnUrl = null)
	{
		await this.signInManager.SignOutAsync();
		this.logger.LogInformation("User logged out.");
		if (returnUrl != null)
		{
			return this.LocalRedirect(returnUrl);
		}
		else
		{
			// This needs to be a redirect so that the browser performs a new
			// request and the identity for the user gets updated.
			return this.RedirectToPage("Login");
		}
	}
}
