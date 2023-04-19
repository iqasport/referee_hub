using ManagementHub.Service.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Tests;

/// <summary>
/// Actions related to testing referees.
/// </summary>
[ApiController]
[Route("api/v2/{controller}")]
[Produces("application/json")]
public class TestsController : ControllerBase
{
	[HttpGet("available")]
	[Authorize(AuthotizationPolicies.RefereePolicy)]
	public Task GetAvailableTests()
	{
		// TODO: return a list of tests that the user can currently make an attempt at
		throw new NotImplementedException();
	}

	[HttpGet("attempts")]
	[Authorize(AuthotizationPolicies.RefereePolicy)]
	public Task GetTestAttempts()
	{
		// TODO: return a list of test attempts/results the user has made
		// TODO: make sure to include any [In Progress] attempts with a link to continue the test
		throw new NotImplementedException();
	}

	// POST start
	// creates a test attempt if user is eligible
	// schedules test finalization

	// POST finish
	// executes test finalization

	// GET continue/{attempt_id}
	// retrieves current test status to continue it (incl scheduled date of termination)

	// POST mark/{attempt_id}/questions/{question_id}/answers/{answer_id}
	// marks the selected answer as selected for the test
}
