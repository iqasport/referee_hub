using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ManagementHub.Service.Areas.Identity;

/// <summary>
/// Copied from https://github.com/dotnet/aspnetcore/blob/8be9c9e80ebb4dfa7dad7b8f6215fb146b2898c2/src/Identity/Core/src/IdentityServiceCollectionExtensions.cs#L187
/// </summary>
internal sealed class CompositeIdentityHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
	: SignInAuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var bearerResult = await this.Context.AuthenticateAsync(IdentityConstants.BearerScheme);

		// Only try to authenticate with the application cookie if there is no bearer token.
		if (!bearerResult.None)
		{
			return bearerResult;
		}

		// Cookie auth will return AuthenticateResult.NoResult() like bearer auth just did if there is no cookie.
		return await this.Context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
	}

	protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
	{
		throw new NotImplementedException();
	}

	protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
	{
		throw new NotImplementedException();
	}
}
