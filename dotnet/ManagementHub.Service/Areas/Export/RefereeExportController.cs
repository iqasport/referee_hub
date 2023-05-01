using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
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

	public RefereeExportController(IUserContextAccessor contextAccessor)
	{
		this.contextAccessor = contextAccessor;
	}

	[HttpPost("{ngb}")]
	[Authorize(AuthorizationPolicies.RefereeViewerPolicy)]
	public async Task ExportRefereesForNgb([FromRoute]NgbIdentifier ngb)
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

		// TODO: schedule a job for exporting referee data for the NGB using the IRefereeContextProvider and sent over email to the current user
		throw new NotImplementedException();
	}
}
