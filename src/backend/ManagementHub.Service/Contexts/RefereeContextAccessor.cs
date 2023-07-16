using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Exceptions;

namespace ManagementHub.Service.Contexts;

public class RefereeContextAccessor : IRefereeContextAccessor
{
	private readonly IHttpContextAccessor httpContextAccessor;
	private readonly IRefereeContextProvider contextProvider;
	private readonly IUserContextAccessor userContextAccessor;

	public RefereeContextAccessor(IHttpContextAccessor httpContextAccessor, IRefereeContextProvider contextProvider, IUserContextAccessor userContextAccessor)
	{
		this.httpContextAccessor = httpContextAccessor;
		this.contextProvider = contextProvider;
		this.userContextAccessor = userContextAccessor;
	}

	public async Task<IRefereeViewContext> GetRefereeViewContextAsync(UserIdentifier userId)
	{
		var currentUser = await this.userContextAccessor.GetCurrentUserContextAsync();
		var ngbConstraint = this.GetNgbConstraint(currentUser);
		return await this.contextProvider.GetRefereeViewContextAsync(userId, ngbConstraint, this.HttpContext.RequestAborted);
	}

	public async Task<IQueryable<IRefereeViewContext>> GetRefereeViewContextListAsync()
	{
		var currentUser = await this.userContextAccessor.GetCurrentUserContextAsync();
		var ngbConstraint = this.GetNgbConstraint(currentUser);
		return this.contextProvider.GetRefereeViewContextQueryable(ngbConstraint);
	}

	public async Task<IRefereeViewContext> GetRefereeViewContextForCurrentUserAsync()
	{
		var currentUser = await this.userContextAccessor.GetCurrentUserContextAsync();
		return await this.contextProvider.GetRefereeViewContextAsync(currentUser.UserId, NgbConstraint.Any, this.HttpContext.RequestAborted);
	}

	public async Task<IRefereeTestContext> GetRefereeTestContextForCurrentUserAsync()
	{
		var currentUser = await this.userContextAccessor.GetCurrentUserContextAsync();
		return await this.contextProvider.GetRefereeTestContextAsync(currentUser.UserId, this.HttpContext.RequestAborted);
	}

	private NgbConstraint GetNgbConstraint(IUserContext currentUser)
	{
		var refereeViewerRole = currentUser.Roles.OfType<RefereeViewerRole>().FirstOrDefault();
		if (refereeViewerRole == null)
		{
			throw new AccessDeniedException(nameof(RefereeViewerRole));
		}

		return refereeViewerRole.Ngb;
	}

	public async Task<IQueryable<IRefereeViewContext>> GetRefereeViewContextListAsync(NgbIdentifier ngbId)
	{
		var currentUser = await this.userContextAccessor.GetCurrentUserContextAsync();
		var ngbUserConstraint = this.GetNgbConstraint(currentUser);
		if (!ngbUserConstraint.AppliesTo(ngbId))
		{
			throw new AccessDeniedException(ngbId.ToString());
		}
		return this.contextProvider.GetRefereeViewContextQueryable(NgbConstraint.Single(ngbId));
	}

	private HttpContext HttpContext => this.httpContextAccessor.HttpContext ?? throw new Exception("Could not retrieve current http context.");
}
