using Hangfire;
using ManagementHub.Models.Abstraction.Commands.Mailers;
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
[Route("api/v2/Ngbs/{ngb}/teams/export")]
[Produces("application/json")]
public class TeamsExportController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly IBackgroundJobClient backgroundJob;
	private readonly ILogger logger;

	public TeamsExportController(IUserContextAccessor contextAccessor, IBackgroundJobClient backgroundJob, ILogger<RefereeExportController> logger)
	{
		this.contextAccessor = contextAccessor;
		this.backgroundJob = backgroundJob;
		this.logger = logger;
	}

	[HttpPost]
	[Tags("Export")]
	[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
	public async Task<ExportResponse> ExportTeamsForNgb([FromRoute] NgbIdentifier ngb)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var ngbAdminRole = userContext.Roles.OfType<NgbAdminRole>().FirstOrDefault();
		if (ngbAdminRole == null)
		{
			throw new AccessDeniedException(nameof(NgbAdminRole));
		}

		if (!ngbAdminRole.Ngb.AppliesTo(ngb))
		{
			throw new AccessDeniedException(ngb.ToString());
		}

		var requestorId = userContext.UserId;
		var jobId = this.backgroundJob.Enqueue<ISendNgbExportEmail>(this.logger, service =>
			service.SendExportTeamsEmailAsync(requestorId, ngb, CancellationToken.None));

		return new ExportResponse
		{
			JobId = jobId,
		};
	}
}
