using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Mailers.Utils;

/// <summary>
/// Email sender which prints the email to the logs - only use locally.
/// </summary>
internal class LogSender : ISender
{
	private readonly ILogger<LogSender> logger;

	public LogSender(ILogger<LogSender> logger)
	{
		this.logger = logger;
	}

	public SendResponse Send(IFluentEmail email, CancellationToken? token = null)
	{
		this.logger.LogInformation("Sending email:\n{email}", email.RenderToString());
		return new SendResponse();
	}

	public Task<SendResponse> SendAsync(IFluentEmail email, CancellationToken? token = null)
	{
		return Task.FromResult(this.Send(email, token));
	}
}
