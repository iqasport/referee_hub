using System.Collections.Generic;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Services;

public interface IUserDelicateInfoService
{
	Task<string?> GetUserGenderAsync(UserIdentifier userId);
	Task SetUserGenderAsync(UserIdentifier userId, string? gender);
	Task<Dictionary<UserIdentifier, string?>> GetMultipleUserGendersAsync(IEnumerable<UserIdentifier> userIds);
}
