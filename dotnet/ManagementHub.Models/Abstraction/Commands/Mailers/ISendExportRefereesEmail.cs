using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Commands.Mailers;
public interface ISendExportRefereesEmail
{
	Task SendExportRefereesEmailAsync(UserIdentifier requestorId, NgbIdentifier ngb, CancellationToken cancellationToken);
}
