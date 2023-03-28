using System.Security.Claims;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Contexts;

public class CurrentContextAccessor : ICurrentContextAccessor
{
	private readonly IHttpContextAccessor httpContextAccessor;
	private readonly IContextProvider contextProvider;

	// cached context so that we don't parse the user id multiple times
	private IUserContext? user;

	public CurrentContextAccessor(
		IHttpContextAccessor httpContextAccessor,
		IContextProvider contextProvider)
	{
		this.httpContextAccessor = httpContextAccessor;
		this.contextProvider = contextProvider;
	}

	public async Task<IUserContext> GetCurrentUserContextAsync()
	{
		if (this.user != null)
		{
			return this.user;
		}

		var httpContext = this.HttpContext;

		var claim = httpContext.User.Claims.SingleOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
		if (claim == null)
		{
			throw new InvalidOperationException("The current HTTP context is missing user information.");
		}

		if (!UserIdentifier.TryParse(claim.Value, out var userId))
		{
			throw new FormatException($"The value in the 'nameidentifier' claim is not a valid {nameof(UserIdentifier)}.");
		}

		this.user = await this.contextProvider.GetUserContextAsync(userId, httpContext.RequestAborted);
		return this.user;
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
