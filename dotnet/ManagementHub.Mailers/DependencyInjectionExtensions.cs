using System;
using System.Net;
using System.Net.Mail;
using FluentEmail.Core.Interfaces;
using FluentEmail.Smtp;
using ManagementHub.Mailers.Commands;
using ManagementHub.Mailers.Configuration;
using ManagementHub.Mailers.Utils;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ManagementHub.Mailers;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddMailers(this IServiceCollection services, bool inMemory)
	{
		services.AddScoped<ISendAccountEmail, SendAccountEmail>();
		services.AddScoped<ISendTestFeedbackEmail, SendTestFeedbackEmail>();
		services.AddScoped<ISendExportRefereesEmail, SendExportRefereesEmail>();

		services.AddOptions<SmtpSettings>()
			.ValidateDataAnnotations()
			.ValidateOnStart()
			.BindConfiguration("Mailers:SMTP");

		services.AddOptions<EmailSenderSettings>()
			.ValidateDataAnnotations()
			.ValidateOnStart()
			.BindConfiguration("Mailers");

		services
			.AddFluentEmail(new EmailSenderSettings().SenderEmail);

		if (inMemory)
		{
			services.AddScoped<ISender, LogSender>();
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
}
