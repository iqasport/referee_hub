using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Abstraction;

public interface IUserRole
{
}

public interface INgbUserRole : IUserRole
{
	public NgbConstraint Ngb { get; }
}
