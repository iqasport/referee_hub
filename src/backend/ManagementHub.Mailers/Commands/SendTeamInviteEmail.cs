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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Mailers.Commands;

internal class SendTeamInviteEmail : ISendTeamInviteEmail
{
	private readonly IFluentEmailFactory emailFactory;
	private readonly ITeamContextProvider teamContextProvider;
	private readonly ILogger<SendTeamInviteEmail> logger;
	private readonly EmailSenderSettings emailSenderSettings;

	public SendTeamInviteEmail(
		IFluentEmailFactory emailFactory,
		ITeamContextProvider teamContextProvider,
		ILogger<SendTeamInviteEmail> logger,
		IOptionsSnapshot<EmailSenderSettings> emailSenderSettings)
	{
		this.emailFactory = emailFactory;
		this.teamContextProvider = teamContextProvider;
		this.logger = logger;
		this.emailSenderSettings = emailSenderSettings.Value;
	}

	public async Task SendTeamInviteEmailAsync(
		TeamIdentifier teamId,
		string invitedEmail,
		string? invitedByName,
		Uri hostUri,
		CancellationToken cancellationToken)
	{
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
		if (team == null)
		{
			this.logger.LogWarning("Cannot send team invite email because team {TeamId} was not found.", SanitizeForLog(teamId.ToString()));
			return;
		}

		var profileUrl = new Uri(hostUri, "/referees/me");
		var subject = $"Team Invitation: {team.TeamData.Name}";
		var inviterLabel = string.IsNullOrWhiteSpace(invitedByName) ? "A team manager" : invitedByName.Trim();
		var body = $@"<html>
<body>
<p>Hello,</p>
<p>{System.Net.WebUtility.HtmlEncode(inviterLabel)} invited you to join <strong>{System.Net.WebUtility.HtmlEncode(team.TeamData.Name)}</strong>.</p>
<p>To accept or decline this invitation, sign in and visit your referee profile:</p>
<p><a href=""{System.Net.WebUtility.HtmlEncode(profileUrl.ToString())}"">Review team invitations</a></p>
<p>Best regards,<br/>
IQA Referee Hub</p>
</body>
</html>";

		await this.emailFactory.Create()
			.SetFrom(this.emailSenderSettings.SenderEmail, this.emailSenderSettings.SenderDisplayName)
			.To(invitedEmail)
			.ReplyTo(this.emailSenderSettings.ReplyToEmail)
			.Subject(subject)
			.Body(body, isHtml: true)
			.SendAsync(cancellationToken);
	}

	public async Task SendTeamInviteResponseEmailAsync(
		TeamIdentifier teamId,
		string responderEmail,
		string? responderName,
		bool approved,
		Uri hostUri,
		CancellationToken cancellationToken)
	{
		var team = await this.teamContextProvider.GetTeamAsync(teamId, NgbConstraint.Any);
		if (team == null)
		{
			this.logger.LogWarning("Cannot send team invite response email because team {TeamId} was not found.", SanitizeForLog(teamId.ToString()));
			return;
		}

		var teamManagers = (await this.teamContextProvider.GetTeamManagersAsync(teamId, NgbConstraint.Any)).ToList();
		if (teamManagers.Count == 0)
		{
			this.logger.LogWarning("Cannot send team invite response email because team {TeamId} has no managers.", SanitizeForLog(teamId.ToString()));
			return;
		}

		var manageUrl = new Uri(hostUri, $"/teams/{teamId}/manage");
		var responderLabel = string.IsNullOrWhiteSpace(responderName) ? responderEmail : responderName.Trim();
		var actionLabel = approved ? "accepted" : "declined";
		var subject = $"Team Invitation {actionLabel}: {team.TeamData.Name}";
		var body = $@"<html>
<body>
<p>Hello,</p>
<p>{System.Net.WebUtility.HtmlEncode(responderLabel)} ({System.Net.WebUtility.HtmlEncode(responderEmail)}) has <strong>{actionLabel}</strong> the invitation to join <strong>{System.Net.WebUtility.HtmlEncode(team.TeamData.Name)}</strong>.</p>
<p>You can review the team membership activity here:</p>
<p><a href=""{System.Net.WebUtility.HtmlEncode(manageUrl.ToString())}"">Open team management</a></p>
<p>Best regards,<br/>
IQA Referee Hub</p>
</body>
</html>";

		var tasks = teamManagers
			.Select(manager => this.emailFactory.Create()
				.SetFrom(this.emailSenderSettings.SenderEmail, this.emailSenderSettings.SenderDisplayName)
				.To(manager.Email)
				.ReplyTo(this.emailSenderSettings.ReplyToEmail)
				.Subject(subject)
				.Body(body, isHtml: true)
				.SendAsync(cancellationToken));

		await Task.WhenAll(tasks);
	}

	private static string SanitizeForLog(string value)
	{
		return value
			.Replace("\r", string.Empty)
			.Replace("\n", string.Empty);
	}
}