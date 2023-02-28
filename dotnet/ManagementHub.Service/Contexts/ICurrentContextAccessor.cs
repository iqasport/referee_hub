using ManagementHub.Models.Abstraction.Contexts;

namespace ManagementHub.Service.Contexts;
public interface ICurrentContextAccessor
{
	Task<IUserContext> GetCurrentUserContextAsync();
}
