// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
	private readonly SignInManager<UserIdentity> signInManager;
	private readonly ILogger<LoginModel> logger;

	public LoginModel(SignInManager<UserIdentity> signInManager, ILogger<LoginModel> logger)
	{
		this.signInManager = signInManager;
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
	public IList<AuthenticationScheme> ExternalLogins { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	public string ReturnUrl { get; set; }

	/// <summary>
	///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
	///     directly from your code. This API may change or be removed in future releases.
	/// </summary>
	[TempData]
	public string ErrorMessage { get; set; }

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

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}

	public async Task OnGetAsync(string returnUrl = null)
	{
		if (!string.IsNullOrEmpty(this.ErrorMessage))
		{
			this.ModelState.AddModelError(string.Empty, this.ErrorMessage);
		}

		returnUrl ??= this.Url.Content("~/");

		// Clear the existing external cookie to ensure a clean login process
		await this.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

		this.ExternalLogins = (await this.signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

		this.ReturnUrl = returnUrl;
	}

	public async Task<IActionResult> OnPostAsync(string returnUrl = null)
	{
		returnUrl ??= this.Url.Content("~/");

		this.ExternalLogins = (await this.signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

		if (this.ModelState.IsValid)
		{
			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, set lockoutOnFailure: true
			var result = await this.signInManager.PasswordSignInAsync(this.Input.Email, this.Input.Password, this.Input.RememberMe, lockoutOnFailure: false);
			if (result.Succeeded)
			{
				this.logger.LogInformation("User logged in.");
				return this.LocalRedirect(returnUrl);
			}
			if (result.RequiresTwoFactor)
			{
				return this.RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, this.Input.RememberMe });
			}
			if (result.IsLockedOut)
			{
				this.logger.LogWarning("User account locked out.");
				return this.RedirectToPage("./Lockout");
			}
			else
			{
				this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
				return this.Page();
			}
		}

		// If we got this far, something failed, redisplay form
		return this.Page();
	}
}
