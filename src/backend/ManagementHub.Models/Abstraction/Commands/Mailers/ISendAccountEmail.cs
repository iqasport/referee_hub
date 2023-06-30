using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Misc;

namespace ManagementHub.Models.Abstraction.Commands.Mailers;
public interface ISendAccountEmail
{
	Task SendAccountEmailAsync(UserIdentifier userId, string subject, [SensitiveData] string htmlMessage, CancellationToken cancellationToken);
}
