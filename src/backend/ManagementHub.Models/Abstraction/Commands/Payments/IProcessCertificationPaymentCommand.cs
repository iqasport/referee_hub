using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Misc;

namespace ManagementHub.Models.Abstraction.Commands.Payments;
public interface IProcessCertificationPaymentCommand
{
	Task ProcessCertificationPaymentAsync(string sessionId, [SensitiveData] string userEmail, Certification item, CancellationToken cancellationToken);
}
