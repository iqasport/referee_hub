using System.Threading.Tasks;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Abstraction.Commands;
public interface IUpdateNgbAdminRoleCommand
{
	public enum AddRoleResult
	{
		UserDoesNotExist,
		RoleAdded,
		UserCreatedWithRole,
	}

	Task<AddRoleResult> AddNgbAdminRoleAsync(NgbIdentifier ngb, Email email, bool createUserIfNotExists);
	Task<bool> DeleteNgbAdminRoleAsync(NgbIdentifier ngb, Email email);
}
