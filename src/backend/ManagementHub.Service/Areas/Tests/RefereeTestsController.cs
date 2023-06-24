using Hangfire;
using ManagementHub.Models.Abstraction.Commands.Mailers;
using ManagementHub.Models.Abstraction.Commands.Tests;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.Tests.Policies;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Misc;
using ManagementHub.Processing.Domain.Tests.Extensions;
using ManagementHub.Processing.Domain.Tests.Policies.Eligibility;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Extensions;
using ManagementHub.Storage.Database.Transactions;
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
	private readonly IDatabaseTransactionProvider databaseTransactionProvider;
	private readonly ILogger logger;
	private readonly ISaveSubmittedTestCommand saveSubmittedTestCommand;
	private readonly IBackgroundJobClient backgroundJob;

	public RefereeTestsController(
		IUserContextAccessor userContextAccessor,
		IRefereeContextAccessor refereeContextAccessor,
		ITestContextProvider testProvider,
		RefereeEligibilityChecker refereeEligibilityChecker,
		IDatabaseTransactionProvider databaseTransactionProvider,
		ILogger<RefereeTestsController> logger,
		ISaveSubmittedTestCommand saveSubmittedTestCommand,
		IBackgroundJobClient backgroundJob)
	{
		this.userContextAccessor = userContextAccessor;
		this.refereeContextAccessor = refereeContextAccessor;
		this.testProvider = testProvider;
		this.refereeEligibilityChecker = refereeEligibilityChecker;
		this.databaseTransactionProvider = databaseTransactionProvider;
		this.logger = logger;
		this.saveSubmittedTestCommand = saveSubmittedTestCommand;
		this.backgroundJob = backgroundJob;
	}

	[HttpGet("available")]
	[Authorize(AuthorizationPolicies.RefereePolicy)]
	public async Task<IEnumerable<RefereeTestAvailableViewModel>> GetAvailableTests()
	{
		// TODO: move logic to a processor
		var user = await this.userContextAccessor.GetCurrentUserContextAsync();
		var tests = await this.testProvider.GetTestsAsync(this.HttpContext.RequestAborted);

		var activeTests = tests.Where(t => t.IsActive).ToList();

		var response = new List<RefereeTestAvailableViewModel>(activeTests.Count);

		foreach (var test in activeTests)
		{
			var eligibilityResult = await this.refereeEligibilityChecker.CheckRefereeEligibilityAsync(test, user.UserId, this.HttpContext.RequestAborted);
			response.Add(new RefereeTestAvailableViewModel
			{
				AwardedCertifications = test.AwardedCertifications,
				IsRefereeEligible = eligibilityResult == RefereeEligibilityResult.Eligible,
				RefereeEligibilityResult = eligibilityResult,
				Language = test.Language,
				TestId = test.TestId,
				Title = test.Title,
			});
		}

		return response;
	}

	[HttpGet("attempts")]
	[Authorize(AuthorizationPolicies.RefereePolicy)]
	public async Task<IEnumerable<TestAttemptViewModel>> GetTestAttempts()
	{
		// FUTURE: make sure to include any [In Progress] attempts with a link to continue the test
		var refereeTestCtx = await this.refereeContextAccessor.GetRefereeTestContextForCurrentUserAsync();
		return refereeTestCtx.TestAttempts.Select(TestAttemptViewModel.FromTestAttempt);
	}

	[HttpPost("{testId}/start")]
	[Authorize(AuthorizationPolicies.RefereePolicy)]
	public async Task<RefereeTestStartModel> StartTest([FromRoute] TestIdentifier testId)
	{
		// TODO: move logic to a processor
		var user = await this.userContextAccessor.GetCurrentUserContextAsync();
		var test = await this.testProvider.GetTestWithQuestionsAsync(testId, this.HttpContext.RequestAborted);

		if (!test.IsActive)
		{
			throw new InvalidOperationException("Cannot start an inactive test.");
		}

		var eligibilityResult = await this.refereeEligibilityChecker.CheckRefereeEligibilityAsync(test, user.UserId, this.HttpContext.RequestAborted);
		if (eligibilityResult != RefereeEligibilityResult.Eligible)
		{
			throw new InvalidOperationException($"User is not eligible to start this test ({eligibilityResult}).");
		}

		// IMPORTANT: we set the random context before choosing questions and shuffling answers for best randomness
		EnumerableExtensions.SetAsyncRandomContext();

		// FUTURE: create in progress attempt
		var questions = test.QuestionChoicePolicy.ChooseQuestions(test.AvailableQuestions);
		var startModel = new RefereeTestStartModel
		{
			Questions = questions.Select(q => new RefereeTestStartModel.Question
			{
				QuestionId = q.QuestionId.Id,
				HtmlText = q.HtmlText,
				Answers = q.Answers.Shuffle().Select(a => new RefereeTestStartModel.Answer
				{
					AnswerId = a.AnswerId.Id,
					HtmlText = a.HtmlText,
				})
			})
		};
		// FUTURE: schedule a job to finilize the test via timeout in TimeLimit + 20 seconds
		return startModel;
	}

	[HttpPost("{testId}/submit")]
	[Authorize(AuthorizationPolicies.RefereePolicy)]
	public async Task<RefereeTestSubmitResponse> SubmitTest([FromRoute] TestIdentifier testId, [FromBody] RefereeTestSubmitModel testSubmitModel)
	{
		// FUTURE: this is a legacy endpoint and will be deprecated once the new finish is implemented.

		// TODO: move logic to a processor
		var now = DateTime.UtcNow; // use the same date throughout operations in this logic

		var user = await this.userContextAccessor.GetCurrentUserContextAsync();
		var test = await this.testProvider.GetTestWithQuestionsAsync(testId, this.HttpContext.RequestAborted);

		if (!test.IsActive)
		{
			throw new InvalidOperationException("Cannot submit an inactive test.");
		}

		var eligibilityResult = await this.refereeEligibilityChecker.CheckRefereeEligibilityAsync(test, user.UserId, this.HttpContext.RequestAborted);
		if (eligibilityResult != RefereeEligibilityResult.Eligible)
		{
			throw new InvalidOperationException($"User is not eligible to start this test ({eligibilityResult}).");
		}

		CertificationLevel highestCertificationLevel = test.AwardedCertifications.Select(c => c.Level).Max();

		var mappedQuestionsWithSelectedAnswers = test.AvailableQuestions.Join(
			testSubmitModel.Answers,
			q => q.QuestionId.Id,
			a => a.QuestionId,
			(q, a) => (question: q, selectedAnswer: q.Answers.FirstOrDefault(aa => a.AnswerId == aa.AnswerId.Id)))
			.Where(p => p.selectedAnswer != null)
			.Cast<(Question question, Answer selectedAnswer)>()
			.DistinctBy(p => p.question.QuestionId)
			.ToList();

		var expectedNumberOfQuestions = ((SubsetCountQuestionChoicePolicy)test.QuestionChoicePolicy).QuestionsCount;
		if (mappedQuestionsWithSelectedAnswers.Count != expectedNumberOfQuestions)
		{
			this.logger.LogWarning(0, "Test has been submitted with an incorrect number of answers. Expected {expected}, but got {submitted}.", expectedNumberOfQuestions, mappedQuestionsWithSelectedAnswers.Count);
			// if more answers have been submitted we only take the first X (at most)
			mappedQuestionsWithSelectedAnswers = mappedQuestionsWithSelectedAnswers.Take(expectedNumberOfQuestions).ToList();
		}

		var scoredPoints = mappedQuestionsWithSelectedAnswers.Select(p => p.question.CorrectAnswers.Contains(p.selectedAnswer) ? p.question.Points : 0).Sum();

		var maxPoints = mappedQuestionsWithSelectedAnswers.Count < expectedNumberOfQuestions
			? mappedQuestionsWithSelectedAnswers.Select(p => p.question.Points).Sum() + (expectedNumberOfQuestions - mappedQuestionsWithSelectedAnswers.Count) // for each missing question we assume 1 point
			: mappedQuestionsWithSelectedAnswers.Select(p => p.question.Points).Sum();

		if (scoredPoints > maxPoints)
		{
			this.logger.LogWarning(0, "Scored points ({scored}) is greater than max points ({max}).", scoredPoints, maxPoints);
			// IMO this shouldn't happen the way I'm calculating points, but if it does, let's cap it and move on rather than block the submission
			scoredPoints = maxPoints;
		}

		Percentage score = 100 * scoredPoints / maxPoints;
		var testDuration = now - testSubmitModel.StartedAt;

		var passed = score >= test.PassPercentage;

		var timeError = TimeSpan.FromSeconds(10); // assuming slow internet and long db access time for the request to get to submission time
		if (testDuration > test.TimeLimit + timeError)
		{
			this.logger.LogWarning(0, "Test has been submitted after time has passed (limit: {timeLimit}, actual: {actual}).", test.TimeLimit, testDuration);
			// previously this resulted in an automatic fail, but it happened once since 2020 rulebook, so I won't action on it
		}

		var testResult = new FinishedTestAttempt
		{
			FinishedAt = now,
			StartedAt = testSubmitModel.StartedAt,
			FinishMethod = TestAttemptFinishMethod.Submission,
			Level = highestCertificationLevel,
			Passed = passed,
			PassPercentage = test.PassPercentage,
			Score = score,
			TestId = test.TestId,
			UserId = user.UserId,
			AwardedCertifications = passed ? test.AwardedCertifications : null,
		};

		await this.saveSubmittedTestCommand.SaveSubmittedTestAsync(
			testResult,
			mappedQuestionsWithSelectedAnswers.Select(p => (p.question.QuestionId, p.selectedAnswer.AnswerId)));

		// if the save to database was successful enqueue the mail
		var attemptId = testResult.Id;
		this.backgroundJob.Enqueue<ISendTestFeedbackEmail>(this.logger, cmd => cmd.SendTestFeedbackEmailAsync(attemptId, CancellationToken.None));

		return new RefereeTestSubmitResponse
		{
			AwardedCertifications = testResult.AwardedCertifications,
			Passed = testResult.Passed,
			PassPercentage = testResult.PassPercentage,
			ScoredPercentage = testResult.Score,
		};
	}

	// FUTURE ENDPOINTS BELOW

	// POST finish
	// executes test finalization

	// GET continue/{attempt_id}
	// retrieves current test status to continue it (incl scheduled date of termination)

	// POST mark/{attempt_id}/questions/{question_id}/answers/{answer_id}
	// marks the selected answer as selected for the test
}
