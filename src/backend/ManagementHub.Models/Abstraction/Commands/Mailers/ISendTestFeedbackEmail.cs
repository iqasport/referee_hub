using System;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Models.Abstraction.Commands.Mailers;

public interface ISendTestFeedbackEmail
{
	public Task SendTestFeedbackEmailAsync(TestAttemptIdentifier testAttemptId, Uri hostUri, bool ccRefhub, CancellationToken cancellation);
}
