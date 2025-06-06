﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using ManagementHub.Mailers.Configuration;
using ManagementHub.Mailers.Models;
using ManagementHub.Mailers.Utils;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Mailers.Commands;

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

	public async Task SendTestFeedbackEmailAsync(TestAttemptIdentifier testAttemptId, Uri hostUri, bool ccRefhub, CancellationToken cancellation)
	{
		try
		{
			this.logger.LogInformation(-0x32943200, "Sending test feedback for test attempt ({attemptId}).", testAttemptId);

			var emailFeedbackContext = await this.refereeContextProvider.GetRefereeEmailFeedbackContextAsync(testAttemptId, cancellation);
			var userContext = await this.userContextProvider.GetUserContextAsync(emailFeedbackContext.TestAttempt.UserId, cancellation);

			this.logger.LogInformation(-0x329431ff, "Sending test feedback to user ({userId}).", userContext.UserId);

			await this.emailFactory.Create()
				.SetFrom(this.emailSenderSettings.SenderEmail, this.emailSenderSettings.SenderDisplayName)
				.To(userContext.UserData.Email.Value)
				.CC(ccRefhub ? [new Address("refhub@iqasport.org")] : Array.Empty<Address>())
				.ReplyTo(this.emailSenderSettings.ReplyToEmail)
				.Subject($"{emailFeedbackContext.Test.Title} Results")
				.UsingEmbeddedTemplate("TestFeedbackEmail", new FeedbackContextWithHostUrl(emailFeedbackContext, hostUri))
				.SendAsync();

			this.logger.LogInformation(-0x329431fe, "Email has been sent.");
		}
		catch (Exception ex)
		{
			this.logger.LogError(-0x329431fd, ex, "Failed to send test feedback.");
			throw;
		}
	}
}
