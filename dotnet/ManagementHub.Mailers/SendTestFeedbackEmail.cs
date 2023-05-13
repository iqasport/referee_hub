using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Mailers;
public class SendTestFeedbackEmail : ISendTestFeedbackEmail
{
	private readonly IFluentEmailFactory emailFactory;

	public SendTestFeedbackEmail(IFluentEmailFactory emailFactory)
	{
		this.emailFactory = emailFactory;
	}

	public async Task SendTestFeedbackEmailAsync(TestAttemptIdentifier testAttemptId, CancellationToken cancellation = default)
	{
		await this.emailFactory.Create()
			.To("test@example.com")
			.Body(testAttemptId.ToString())
			.SendAsync();
	}
}
