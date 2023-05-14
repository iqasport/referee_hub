using System.Security.Claims;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;

namespace ManagementHub.Service.Contexts;

public class UserContextAccessor : IUserContextAccessor
{
	private readonly IHttpContextAccessor httpContextAccessor;
	private readonly IUserContextProvider contextProvider;

	public UserContextAccessor(
		IHttpContextAccessor httpContextAccessor,
		IUserContextProvider contextProvider)
	{
		this.httpContextAccessor = httpContextAccessor;
		this.contextProvider = contextProvider;
	}

	public async Task<IUserContext> GetCurrentUserContextAsync()
	{
		var httpContext = this.HttpContext;

		// In usual scenario there's just one claim like this.
		// But in an impersonation scenario we're going to add an identity and use the last claim as the desired userId.
		var claim = httpContext.User.Claims.LastOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
		if (claim == null)
		{
			throw new AuthenticationRequiredException("The current HTTP context is missing user information.");
		}

		if (!UserIdentifier.TryParse(claim.Value, out var userId))
		{
			throw new FormatException($"The value in the 'nameidentifier' claim is not a valid {nameof(UserIdentifier)}.");
		}

		return await this.contextProvider.GetUserContextAsync(userId, httpContext.RequestAborted);
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

	public Task<IUserContext> GetUserContextAsync(UserIdentifier userId)
	{
		var httpContext = this.HttpContext;
		
		return this.contextProvider.GetUserContextAsync(userId, httpContext.RequestAborted);
	}

	private HttpContext HttpContext => this.httpContextAccessor.HttpContext ?? throw new Exception("Could not retrieve current http context.");
}
