using Hangfire;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Debug;

/// <summary>
/// Actions related to exporting users with the referee role.
/// </summary>
[ApiController]
[Route("api/debug/attempts")]
[Authorize(AuthorizationPolicies.TechAdminPolicy)]
public class AttemptsController : ControllerBase
{
	private readonly ILogger<AttemptsController> logger;
	private readonly IBackgroundJobClient backgroundJobClient;

	public AttemptsController(ILogger<AttemptsController> logger, IBackgroundJobClient backgroundJobClient)
	{
		this.logger = logger;
		this.backgroundJobClient = backgroundJobClient;
	}

	[HttpPost("{attemptId}/resend")]
	[Tags("Debug")]
	public string ResendTestFeedbackEmail([FromRoute] TestAttemptIdentifier attemptId)
	{
		var hostUri = this.GetHostBaseUri();
		var jobId = this.backgroundJobClient.Enqueue<ISendTestFeedbackEmail>(this.logger, cmd => cmd.SendTestFeedbackEmailAsync(attemptId, hostUri, true, CancellationToken.None));
		return jobId;
	}
}
