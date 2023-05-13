using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core.Models;
using FluentEmail.Smtp;
using ManagementHub.Mailers.Configuration;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Mailers;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddMailers(this IServiceCollection services, bool inMemory)
	{
		services.AddScoped<ISendTestFeedbackEmail, SendTestFeedbackEmail>();

		services.AddOptions<SmtpSettings>()
			.ValidateDataAnnotations()
			.ValidateOnStart()
			.BindConfiguration("Mailers:SMTP");

		services.AddOptions<EmailSenderSettings>()
			.ValidateDataAnnotations()
			.ValidateOnStart()
			.BindConfiguration("Mailers");

		services
			.AddFluentEmail(new EmailSenderSettings().EmailSender)
			.AddRazorRenderer();

		if (inMemory)
		{
			services.AddScoped<ISender>()
		}
		else
		{
			services.AddScoped<ISender>((IServiceProvider provider) =>
			{
				var settings = provider.GetRequiredService<IOptionsSnapshot<SmtpSettings>>().Value;
				var client = new SmtpClient()
				{
					Host = settings.Host!,
					Port = settings.Port,
					EnableSsl = settings.EnableSsl,
					DeliveryFormat = SmtpDeliveryFormat.International,
					Timeout = settings.TimeoutInMilliseconds,
					UseDefaultCredentials = false,
				};

				if (settings.Username != null && settings.Password != null)
				{
					client.Credentials = new NetworkCredential(settings.Username, settings.Password);
				}

				if (settings.EnableSsl)
				{
					// TODO: is this required? Supposedly avoids MustIssueStartTlsFirst exception.
					client.TargetName = $"STARTTLS/{settings.Host}";
				}

				return new SmtpSender(client);
			});
		}

		return services;
	}

	private class LogSender : ISender
	{
		private readonly ILogger<LogSender> logger;

		public LogSender(ILogger<LogSender> logger)
		{
			this.logger = logger;
		}

		public SendResponse Send(IFluentEmail email, CancellationToken? token = null)
		{
			this.logger.LogInformation("Sending email:\n{email}", email);
			return new SendResponse();
		}

		public Task<SendResponse> SendAsync(IFluentEmail email, CancellationToken? token = null)
		{
			return Task.FromResult(this.Send(email, token));
		}
	}
}
