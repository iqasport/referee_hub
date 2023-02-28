using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Contexts;

public class CurrentContextAccessor : ICurrentContextAccessor
{
	public const string UserIdentifierClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
	private readonly IHttpContextAccessor httpContextAccessor;
	private readonly IContextProvider contextProvider;

	public CurrentContextAccessor(
		IHttpContextAccessor httpContextAccessor,
		IContextProvider contextProvider)
	{
		this.httpContextAccessor = httpContextAccessor;
		this.contextProvider = contextProvider;
	}

	public Task<IUserContext> GetCurrentUserContextAsync()
	{
		var httpContext = this.httpContextAccessor.HttpContext;
		if (httpContext == null)
		{
			throw new Exception("Could not retrieve current http context.");
		}

		var claim = httpContext.User.Claims.SingleOrDefault(claim => claim.Type == UserIdentifierClaimType);
		if (claim == null)
		{
			throw new Exception("The current HTTP context is missing user information.");
		}

		if (!UserIdentifier.TryParse(claim.Value, out var userId))
		{
			throw new Exception($"The value in the 'nameidentifier' claim is not a valid {nameof(UserIdentifier)}.");
		}

		return this.contextProvider.GetUserContextAsync(userId, httpContext.RequestAborted);
	}
}
