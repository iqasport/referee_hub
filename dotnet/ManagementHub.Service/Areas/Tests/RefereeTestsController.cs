using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Processing.Domain.Tests.Policies.Eligibility;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Tests;

/// <summary>
/// Actions related to testing referees.
/// </summary>
[ApiController]
[Route("api/v2/referees/me/tests")]
[Produces("application/json")]
public class RefereeTestsController : ControllerBase
{
	private readonly IUserContextAccessor userContextAccessor;
	private readonly IRefereeContextAccessor refereeContextAccessor;
	private readonly ITestContextProvider testProvider;
	private readonly RefereeEligibilityChecker refereeEligibilityChecker;

	public RefereeTestsController(IUserContextAccessor userContextAccessor, IRefereeContextAccessor refereeContextAccessor, ITestContextProvider testProvider, RefereeEligibilityChecker refereeEligibilityChecker)
	{
		this.userContextAccessor = userContextAccessor;
		this.refereeContextAccessor = refereeContextAccessor;
		this.testProvider = testProvider;
		this.refereeEligibilityChecker = refereeEligibilityChecker;
	}

	[HttpGet("available")]
	[Authorize(AuthotizationPolicies.RefereePolicy)]
	public async Task<IEnumerable<RefereeTestAvailableViewModel>> GetAvailableTests()
	{
		// TODO: move logic to a processor
		var user = await this.userContextAccessor.GetCurrentUserContextAsync();
		var tests = await this.testProvider.GetTestsAsync(this.HttpContext.RequestAborted);

		var activeTests = tests.Where(t => t.IsActive).ToList();

		var response = new List<RefereeTestAvailableViewModel>(activeTests.Count);
		
		foreach (var test in activeTests)
		{
			var isRefereeEligible = await this.refereeEligibilityChecker.CheckRefereeEligibilityAsync(test, user.UserId, this.HttpContext.RequestAborted);
			response.Add(new RefereeTestAvailableViewModel
			{
				AwardedCertifications = test.AwardedCertifications,
				IsRefereeEligible = isRefereeEligible,
				Language = test.Language,
				TestId = test.TestId,
				Title = test.Title,
			});
		}

		return response;
	}

	[HttpGet("attempts")]
	[Authorize(AuthotizationPolicies.RefereePolicy)]
	public async Task<IEnumerable<TestAttempt>> GetTestAttempts()
	{
		// TODO: return a list of test attempts/results the user has made (via view model)
		// TODO: make sure to include any [In Progress] attempts with a link to continue the test
		var refereeTestCtx = await this.refereeContextAccessor.GetRefereeTestContextForCurrentUserAsync();
		return refereeTestCtx.TestAttempts;
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
