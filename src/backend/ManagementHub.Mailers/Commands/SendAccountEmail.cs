using System;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using ManagementHub.Mailers.Configuration;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Mailers.Commands;

internal class SendAccountEmail : ISendAccountEmail
{
	private readonly IFluentEmailFactory emailFactory;
	private readonly IUserContextProvider userContextProvider;
	private readonly ILogger<SendExportRefereesEmail> logger;
	private readonly EmailSenderSettings emailSenderSettings;

	public SendAccountEmail(
		IFluentEmailFactory emailFactory,
		IUserContextProvider userContextProvider,
		ILogger<SendExportRefereesEmail> logger,
		IOptionsSnapshot<EmailSenderSettings> emailSenderSettings)
	{
		this.emailFactory = emailFactory;
		this.userContextProvider = userContextProvider;
		this.logger = logger;
		this.emailSenderSettings = emailSenderSettings.Value;
	}

	public async Task SendAccountEmailAsync(UserIdentifier userId, string subject, [SensitiveData] string htmlMessage, CancellationToken cancellationToken)
	{
		try
		{
			this.logger.LogInformation(0, "Sending account related email to user ({userId}) with subject '{subject}'.", userId, subject);

			var userContext = await this.userContextProvider.GetUserContextAsync(userId, cancellationToken);

			await this.emailFactory.Create()
				.SetFrom(this.emailSenderSettings.SenderEmail, this.emailSenderSettings.SenderDisplayName)
				.To(userContext.UserData.Email.Value)
				.ReplyTo(this.emailSenderSettings.ReplyToEmail)
				.Subject(subject)
				.Body(htmlMessage, isHtml: true)
				.SendAsync();
		}
		catch (Exception ex)
		{
			this.logger.LogError(0, ex, "Failed to send account email.");
			throw;
		}
	}


}