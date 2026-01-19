using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using ManagementHub.Mailers.Configuration;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Mailers.Commands;

internal class SendTournamentContactEmail : ISendTournamentContactEmail
{
	private readonly IFluentEmailFactory emailFactory;
	private readonly IUserContextProvider userContextProvider;
	private readonly ITournamentContextProvider tournamentContextProvider;
	private readonly ILogger<SendTournamentContactEmail> logger;
	private readonly EmailSenderSettings emailSenderSettings;

	public SendTournamentContactEmail(
		IFluentEmailFactory emailFactory,
		IUserContextProvider userContextProvider,
		ITournamentContextProvider tournamentContextProvider,
		ILogger<SendTournamentContactEmail> logger,
		IOptionsSnapshot<EmailSenderSettings> emailSenderSettings)
	{
		this.emailFactory = emailFactory;
		this.userContextProvider = userContextProvider;
		this.tournamentContextProvider = tournamentContextProvider;
		this.logger = logger;
		this.emailSenderSettings = emailSenderSettings.Value;
	}

	public async Task SendTournamentContactEmailAsync(
		TournamentIdentifier tournamentId,
		UserIdentifier senderId,
		[SensitiveData] string message,
		CancellationToken cancellationToken)
	{
		try
		{
			this.logger.LogInformation(0x7a1bc400, "Sending tournament contact email for tournament {TournamentId} from user {SenderId}.", tournamentId, senderId);

			// Get sender info
			var senderContext = await this.userContextProvider.GetUserContextAsync(senderId, cancellationToken);

			// Get tournament context to get tournament name
			var tournament = await this.tournamentContextProvider.GetTournamentContextAsync(tournamentId, senderId, cancellationToken);

			// Get all tournament managers
			var managers = await this.tournamentContextProvider.GetTournamentManagersAsync(tournamentId, cancellationToken);
			var managerEmails = managers.Select(m => m.Email).ToList();

			if (!managerEmails.Any())
			{
				this.logger.LogWarning(0x7a1bc401, "No managers found for tournament {TournamentId}.", tournamentId);
				return;
			}

			this.logger.LogInformation(0x7a1bc402, "Sending tournament contact email to {ManagerCount} manager(s).", managerEmails.Count);

			// Create the email message
			var subject = $"Tournament Contact: {tournament.Name}";
			var body = $@"<html>
<body>
<p>You have received a message regarding the tournament <strong>{tournament.Name}</strong>:</p>
<hr />
<p>{System.Net.WebUtility.HtmlEncode(message).Replace("\n", "<br />")}</p>
<hr />
<p>This message was sent by {System.Net.WebUtility.HtmlEncode($"{senderContext.UserData.FirstName} {senderContext.UserData.LastName}")} ({System.Net.WebUtility.HtmlEncode(senderContext.UserData.Email.Value)}).</p>
<p>You can reply directly to this email to respond to the sender.</p>
</body>
</html>";

			// Send email to each manager individually with reply-to set to sender
			foreach (var managerEmail in managerEmails)
			{
				await this.emailFactory.Create()
					.SetFrom(this.emailSenderSettings.SenderEmail, this.emailSenderSettings.SenderDisplayName)
					.To(managerEmail)
					.ReplyTo(senderContext.UserData.Email.Value)
					.Subject(subject)
					.Body(body, isHtml: true)
					.SendAsync();
			}

			this.logger.LogInformation(0x7a1bc403, "Tournament contact email sent successfully.");
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x7a1bc404, ex, "Failed to send tournament contact email.");
			throw;
		}
	}
}
