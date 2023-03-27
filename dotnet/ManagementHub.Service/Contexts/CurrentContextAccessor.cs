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
		var httpContext = this.HttpContext;

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


	public Task<IUserDataContext> GetUserDataContextAsync(UserIdentifier userId)
	{
		var httpContext = this.HttpContext;
		
		return this.contextProvider.GetUserDataContextAsync(userId, httpContext.RequestAborted);
	}

	public Task<IUserAvatarContext> GetUserAvatarContextAsync(UserIdentifier userId)
	{
		var httpContext = this.HttpContext;

		return this.contextProvider.GetUserAvatarContextAsync(userId, httpContext.RequestAborted);
	}

	private HttpContext HttpContext => this.httpContextAccessor.HttpContext ?? throw new Exception("Could not retrieve current http context.");
}
