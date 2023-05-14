using System;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using ManagementHub.Mailers.Configuration;
using ManagementHub.Mailers.Utils;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Mailers;

internal class SendTestFeedbackEmail : ISendTestFeedbackEmail
{
	private readonly IFluentEmailFactory emailFactory;
	private readonly IUserContextProvider userContextProvider;
	private readonly IRefereeContextProvider refereeContextProvider;
	private readonly ILogger<SendTestFeedbackEmail> logger;
	private readonly EmailSenderSettings emailSenderSettings;

	public SendTestFeedbackEmail(
		IFluentEmailFactory emailFactory,
		IUserContextProvider userContextProvider,
		IRefereeContextProvider refereeContextProvider,
		ILogger<SendTestFeedbackEmail> logger,
		IOptionsSnapshot<EmailSenderSettings> emailSenderSettings)
	{
		this.emailFactory = emailFactory;
		this.userContextProvider = userContextProvider;
		this.refereeContextProvider = refereeContextProvider;
		this.logger = logger;
		this.emailSenderSettings = emailSenderSettings.Value;
	}

	public async Task SendTestFeedbackEmailAsync(TestAttemptIdentifier testAttemptId, CancellationToken cancellation)
	{
		try
		{
			this.logger.LogInformation(0, "Sending test feedback for test attempt ({attemptId}).", testAttemptId);

			var emailFeedbackContext = await this.refereeContextProvider.GetRefereeEmailFeedbackContextAsync(testAttemptId, cancellation);
			var userContext = await this.userContextProvider.GetUserContextAsync(emailFeedbackContext.TestAttempt.UserId, cancellation);
			
			this.logger.LogInformation(0, "Sending test feedback to user ({userId}).", userContext.UserId);

			await this.emailFactory.Create()
				.SetFrom(this.emailSenderSettings.SenderEmail, this.emailSenderSettings.SenderDisplayName)
				.To(userContext.UserData.Email.Value)
				.ReplyTo(this.emailSenderSettings.ReplyToEmail)
				.Subject($"{emailFeedbackContext.Test.Title} Results")
				.UsingEmbeddedTemplate("TestFeedbackEmail", emailFeedbackContext)
				.SendAsync();

			this.logger.LogInformation(0, "Email has been sent.");
		}
		catch (Exception ex)
		{
			this.logger.LogError(0, ex, "Failed to send test feedback.");
			throw;
		}
	}
}
