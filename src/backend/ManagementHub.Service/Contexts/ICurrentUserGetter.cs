using ManagementHub.Models.Domain.User;

namespace ManagementHub.Service.Contexts;

public interface ICurrentUserGetter
{
	UserIdentifier CurrentUser { get; }
}
