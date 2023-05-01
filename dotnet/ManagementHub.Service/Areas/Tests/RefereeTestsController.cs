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

	[HttpPost("{testId}/start")]
	[Authorize(AuthotizationPolicies.RefereePolicy)]
	public async Task StartTest([FromRoute] TestIdentifier testId)
	{
		// TODO: move logic to a processor
		var user = await this.userContextAccessor.GetCurrentUserContextAsync();
		var test = await this.testProvider.GetTestWithQuestionsAsync(testId, this.HttpContext.RequestAborted);

		if (!test.IsActive)
		{
			throw new InvalidOperationException("Cannot start an inactive test.");
		}

		var isRefereeEligible = await this.refereeEligibilityChecker.CheckRefereeEligibilityAsync(test, user.UserId, this.HttpContext.RequestAborted);
		if (!isRefereeEligible)
		{
			throw new InvalidOperationException("User is not eligible to start this test.");
		}

		// FUTURE: create in progress attempt
		var questions = test.QuestionChoicePolicy.ChooseQuestions(test.AvailableQuestions);
		// TODO: mix answers order here or in UX?
		// FUTURE: schedule a job to finilize the test via timeout in TimeLimit + 20 seconds
		return; // TODO: create view model and remove the "correctAnswer" from questions
	}

	// POST finish
	// executes test finalization

	// FUTURE ENDPOINTS BELOW

	// GET continue/{attempt_id}
	// retrieves current test status to continue it (incl scheduled date of termination)

	// POST mark/{attempt_id}/questions/{question_id}/answers/{answer_id}
	// marks the selected answer as selected for the test
}
