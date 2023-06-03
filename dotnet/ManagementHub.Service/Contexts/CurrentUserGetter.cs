using System.Diagnostics;
using System.Security.Claims;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;
using OpenTelemetry;

namespace ManagementHub.Service.Contexts;

public class CurrentUserGetter : ICurrentUserGetter
{
	private readonly IHttpContextAccessor httpContextAccessor;

	public CurrentUserGetter(IHttpContextAccessor httpContextAccessor)
	{
		this.httpContextAccessor = httpContextAccessor;
	}

	public UserIdentifier CurrentUser => this.GetUser();

	private UserIdentifier GetUser()
	{
		string userIdToTryParse = string.Empty;
		var httpContext = this.httpContextAccessor.HttpContext;
		if (httpContext != null)
		{
			// In usual scenario there's just one claim like this.
			// But in an impersonation scenario we're going to add an identity and use the last claim as the desired userId.
			var claim = httpContext.User.Claims.LastOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
			if (claim == null)
			{
				throw new AuthenticationRequiredException("The current HTTP context is missing user information.");
			}

			userIdToTryParse = claim.Value;
		}
		else
		{
			var userFromActivity = Activity.Current?.GetBaggageItem("user_id");
			if (userFromActivity != null)
			{
				userIdToTryParse = userFromActivity;
			}
		}

		if (!UserIdentifier.TryParse(userIdToTryParse, out var userId))
		{
			throw new FormatException($"The value in the 'nameidentifier' claim is not a valid {nameof(UserIdentifier)}. Clear cookies and log in again.");
		}

		Activity.Current?.AddTag("user.id", userId.ToString());
		Baggage.SetBaggage("user_id", userId.ToString());

		return userId;
	}
}
