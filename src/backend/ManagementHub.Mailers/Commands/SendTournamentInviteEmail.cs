using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using ManagementHub.Mailers.Configuration;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Mailers.Commands;

internal class SendTournamentInviteEmail : ISendTournamentInviteEmail
{
	private readonly IFluentEmailFactory emailFactory;
	private readonly ITournamentContextProvider tournamentContextProvider;
	private readonly ITeamContextProvider teamContextProvider;
	private readonly ILogger<SendTournamentInviteEmail> logger;
	private readonly EmailSenderSettings emailSenderSettings;

	public SendTournamentInviteEmail(
		IFluentEmailFactory emailFactory,
		ITournamentContextProvider tournamentContextProvider,
		ITeamContextProvider teamContextProvider,
		ILogger<SendTournamentInviteEmail> logger,
		IOptionsSnapshot<EmailSenderSettings> emailSenderSettings)
	{
		this.emailFactory = emailFactory;
		this.tournamentContextProvider = tournamentContextProvider;
		this.teamContextProvider = teamContextProvider;
		this.logger = logger;
		this.emailSenderSettings = emailSenderSettings.Value;
	}

	public async Task SendTournamentInviteEmailAsync(
		TournamentIdentifier tournamentId,
		TeamIdentifier teamId,
		Uri hostUri,
		CancellationToken cancellationToken)
	{
		try
		{
			this.logger.LogInformation(0x7a1bc500, "Sending tournament invite email for tournament {TournamentId} to team {TeamId}.", tournamentId, teamId);

			// Get team managers who will receive the invitation email
			var teamManagers = await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Any);
			var managersList = teamManagers.ToList();

			if (!managersList.Any())
			{
				this.logger.LogWarning(0x7a1bc501, "No team managers found for team {TeamId}. Cannot send invitation email.", teamId);
				return;
			}

			// Get tournament context - we need any valid user ID, so we use the first manager's ID
			var tournament = await this.tournamentContextProvider.GetTournamentContextAsync(
				tournamentId,
				managersList[0].UserId,
				cancellationToken);

			// Get team info
			var teamContext = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
			if (teamContext == null)
			{
				this.logger.LogWarning(0x7a1bc502, "Team {TeamId} not found. Cannot send invitation email.", teamId);
				return;
			}

			var teamName = teamContext.TeamData.Name;

			this.logger.LogInformation(0x7a1bc503, "Sending tournament invite email to {ManagerCount} team manager(s).", managersList.Count);

			// Build the tournament URL
			var tournamentUrl = new Uri(hostUri, $"/tournaments/{tournamentId}");

			// Create the email message
			var subject = $"Tournament Invitation: {tournament.Name}";
			var body = $@"<html>
<body>
<p>Hello,</p>
<p>Your team <strong>{System.Net.WebUtility.HtmlEncode(teamName)}</strong> has been invited to participate in the tournament <strong>{System.Net.WebUtility.HtmlEncode(tournament.Name)}</strong>.</p>
<p><strong>Tournament Details:</strong></p>
<ul>
<li>Name: {System.Net.WebUtility.HtmlEncode(tournament.Name)}</li>
<li>Dates: {tournament.StartDate:yyyy-MM-dd} to {tournament.EndDate:yyyy-MM-dd}</li>
<li>Location: {System.Net.WebUtility.HtmlEncode(tournament.City)}, {System.Net.WebUtility.HtmlEncode(tournament.Country)}</li>
</ul>
<p>To accept or decline this invitation, please visit the tournament page:</p>
<p><a href=""{System.Net.WebUtility.HtmlEncode(tournamentUrl.ToString())}"">View Tournament and Respond to Invitation</a></p>
<p>If you have any questions, please contact the tournament organizer.</p>
<p>Best regards,<br/>
IQA Referee Hub</p>
</body>
</html>";

			// Send email to each team manager concurrently
			var emailTasks = managersList.Select(manager =>
				this.emailFactory.Create()
					.SetFrom(this.emailSenderSettings.SenderEmail, this.emailSenderSettings.SenderDisplayName)
					.To(manager.Email)
					.ReplyTo(this.emailSenderSettings.ReplyToEmail)
					.Subject(subject)
					.Body(body, isHtml: true)
					.SendAsync()
			).ToList();

			await Task.WhenAll(emailTasks);

			this.logger.LogInformation(0x7a1bc505, "Tournament invite email sent successfully.");
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x7a1bc506, ex, "Failed to send tournament invite email.");
			throw;
		}
	}
}
