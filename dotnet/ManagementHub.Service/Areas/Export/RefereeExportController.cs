using Hangfire;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Export;

/// <summary>
/// Actions related to exporting users with the referee role.
/// </summary>
[ApiController]
[Route("api/v2/referees/export")]
[Produces("application/json")]
public class RefereeExportController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly IBackgroundJobClient backgroundJob;
	private readonly ILogger logger;

	public RefereeExportController(IUserContextAccessor contextAccessor, IBackgroundJobClient backgroundJob, ILogger<RefereeExportController> logger)
	{
		this.contextAccessor = contextAccessor;
		this.backgroundJob = backgroundJob;
		this.logger = logger;
	}

	[HttpPost("{ngb}")]
	[Authorize(AuthorizationPolicies.RefereeViewerPolicy)]
	public async Task ExportRefereesForNgb([FromRoute] NgbIdentifier ngb)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var refereeViewerRole = userContext.Roles.OfType<RefereeViewerRole>().FirstOrDefault();
		if (refereeViewerRole == null)
		{
			throw new AccessDeniedException(nameof(RefereeViewerRole));
		}

		if (!refereeViewerRole.Ngb.AppliesTo(ngb))
		{
			throw new AccessDeniedException(ngb.ToString());
		}

		var requestorId = userContext.UserId;
		var jobId = this.backgroundJob.Enqueue<ISendExportRefereesEmail>(this.logger, service =>
			service.SendExportRefereesEmailAsync(requestorId, ngb, CancellationToken.None));

		var response = new { JobId = jobId };
		return; // TODO model and return
	}
}
