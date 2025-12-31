using System.Threading.Tasks;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IUpdateTeamManagerRoleCommand
{
	public enum AddRoleResult
	{
		UserDoesNotExist,
		RoleAdded,
		UserCreatedWithRole,
	}

	Task<AddRoleResult> AddTeamManagerRoleAsync(
		TeamIdentifier teamId,
		Email email,
		bool createUserIfNotExists,
		UserIdentifier addedByUserId);

	Task<bool> DeleteTeamManagerRoleAsync(TeamIdentifier teamId, Email email);
}
