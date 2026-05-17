using System.Threading.Tasks;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IUpdateNgbAdminRoleCommand
{
	public enum AddRoleResult
	{
		UserDoesNotExist,
		RoleAdded,
		UserCreatedWithRole,
	}

	public record struct AddRoleResponse(AddRoleResult Result, UserIdentifier? UserId);

	Task<AddRoleResponse> AddNgbAdminRoleAsync(NgbIdentifier ngb, Email email, bool createUserIfNotExists);
	Task<bool> DeleteNgbAdminRoleAsync(NgbIdentifier ngb, Email email);
}
