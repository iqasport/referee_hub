using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using ManagementHub.Mailers.Configuration;
using ManagementHub.Mailers.Utils;
using ManagementHub.Models.Abstraction.Commands.Export;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Mailers.Commands;

internal class SendExportRefereesEmail : ISendExportRefereesEmail
{
	private readonly IFluentEmailFactory emailFactory;
	private readonly IUserContextProvider userContextProvider;
	private readonly IExportRefereesToCsv exportRefereesToCsv;
	private readonly ILogger<SendExportRefereesEmail> logger;
	private readonly EmailSenderSettings emailSenderSettings;

	public SendExportRefereesEmail(
		IFluentEmailFactory emailFactory,
		IUserContextProvider userContextProvider,
		IExportRefereesToCsv exportRefereesToCsv,
		ILogger<SendExportRefereesEmail> logger,
		IOptionsSnapshot<EmailSenderSettings> emailSenderSettings)
	{
		this.emailFactory = emailFactory;
		this.userContextProvider = userContextProvider;
		this.exportRefereesToCsv = exportRefereesToCsv;
		this.logger = logger;
		this.emailSenderSettings = emailSenderSettings.Value;
	}

	public async Task SendExportRefereesEmailAsync(UserIdentifier requestorId, NgbIdentifier ngb, CancellationToken cancellationToken)
	{
		try
		{
			this.logger.LogInformation(0, "Exporting referees of NGB {ngb} requested by ({userId}).", ngb, requestorId);

			var userContext = await this.userContextProvider.GetUserContextAsync(requestorId, cancellationToken);
			var refereeViewerRole = userContext.Roles.OfType<RefereeViewerRole>().FirstOrDefault();
			if (refereeViewerRole == null)
			{
				this.logger.LogError(0, $"User is not authorized - missing {nameof(RefereeViewerRole)}.");
				return;
			}

			if (!refereeViewerRole.Ngb.AppliesTo(ngb))
			{
				this.logger.LogError(0, $"User is not authorized - no access to NGB {ngb}.", ngb);
				return;
			}

			using var attachmentStream = this.exportRefereesToCsv.ExportRefereesAsync(NgbConstraint.Single(ngb), cancellationToken);

			var templateData = new
			{
				User = userContext.UserData,
			};

			await this.emailFactory.Create()
				.SetFrom(this.emailSenderSettings.SenderEmail, this.emailSenderSettings.SenderDisplayName)
				.To(userContext.UserData.Email.Value)
				.ReplyTo(this.emailSenderSettings.ReplyToEmail)
				.Subject($"Your Referee Export is ready")
				.UsingEmbeddedTemplate("CsvExportEmail", templateData)
				.Attach(new FluentEmail.Core.Models.Attachment
				{
					Filename = $"RefereeExport_{ngb}_{DateTime.UtcNow.Date:yyyyMMdd}.csv",
					Data = attachmentStream,
					ContentType = "text/csv",
				})
				.SendAsync();
		}
		catch (Exception ex)
		{
			this.logger.LogError(0, ex, "Failed to export referees.");
			throw;
		}
	}


}
