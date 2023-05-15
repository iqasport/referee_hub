using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using ManagementHub.Mailers.Configuration;
using ManagementHub.Mailers.Utils;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Domain.User.Roles;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Mailers.Commands;

internal class SendExportRefereesEmail : ISendExportRefereesEmail
{
    private readonly IFluentEmailFactory emailFactory;
	private readonly IUserContextProvider userContextProvider;
	private readonly IRefereeContextProvider refereeContextProvider;
	private readonly ILogger<SendExportRefereesEmail> logger;
	private readonly EmailSenderSettings emailSenderSettings;

	public SendExportRefereesEmail(
        IFluentEmailFactory emailFactory,
        IUserContextProvider userContextProvider,
        IRefereeContextProvider refereeContextProvider,
        ILogger<SendExportRefereesEmail> logger,
        EmailSenderSettings emailSenderSettings)
	{
		this.emailFactory = emailFactory;
		this.userContextProvider = userContextProvider;
		this.refereeContextProvider = refereeContextProvider;
		this.logger = logger;
		this.emailSenderSettings = emailSenderSettings;
	}

	public async Task SendExportRefereesEmailAsync(UserIdentifier requestorId, NgbIdentifier ngb, CancellationToken cancellationToken)
	{
        try
		{
			this.logger.LogInformation(0, "Exporting referees of NGB {ngb} requested by ({userId}).", ngb, requestorId);

            // TODO: move all this to another background job for CSV processing before email job is invoked
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

            var referees = this.refereeContextProvider.GetRefereeViewContextQueryable(NgbConstraint.Single(ngb));
            var teams = new Dictionary<TeamIdentifier, string>(); // TODO: load teams to get their names

            // TODO: build CSV file and upload
            // TODO: save record in dbcontext
            // TODO: invoke this mailer job
            // TODO: check what is the link in the db - is it time bound? see the buckets in AWS "@userId-exports"?

            var export = new { Type = "Referee Export" }; // TODO: load export from db

            await this.emailFactory.Create()
				.SetFrom(this.emailSenderSettings.SenderEmail, this.emailSenderSettings.SenderDisplayName)
				.To(userContext.UserData.Email.Value)
				.ReplyTo(this.emailSenderSettings.ReplyToEmail)
				.Subject($"Your {export.Type} is ready")
				.UsingEmbeddedTemplate("CsvExportEmail", export) // TODO: align model with view
				.SendAsync();
        }
		catch (Exception ex)
		{
			this.logger.LogError(0, ex, "Failed to export referees.");
			throw;
		}
	}

    private class CsvRow
    {
        public required string Name {get;set;}
        public required string Teams {get;set;}
        public required string Certifications {get;set;} 
    }
}