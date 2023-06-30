using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Contexts;

public class UserContextAccessor : IUserContextAccessor
{
	private readonly IHttpContextAccessor httpContextAccessor;
	private readonly IUserContextProvider contextProvider;
	private readonly ICurrentUserGetter userGetter;

	public UserContextAccessor(
		IHttpContextAccessor httpContextAccessor,
		IUserContextProvider contextProvider,
		ICurrentUserGetter userGetter)
	{
		this.httpContextAccessor = httpContextAccessor;
		this.contextProvider = contextProvider;
		this.userGetter = userGetter;
	}

	public async Task<IUserContext> GetCurrentUserContextAsync()
	{
		var httpContext = this.HttpContext;

		var userId = this.userGetter.CurrentUser;

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
