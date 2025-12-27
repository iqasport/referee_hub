using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Commands;

public interface ISetUserAttributeCommand
{
	Task SetRootUserAttributeAsync(UserIdentifier userId, string key, JsonDocument document, CancellationToken cancellationToken);
	Task SetUserAttributeAsync(UserIdentifier userId, NgbIdentifier ngb, string key, JsonDocument document, CancellationToken cancellationToken);
}
