using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Referee;
public class DbRefereeEmailFeedbackContext : IRefereeEmailFeedbackContext
{
	public required Models.Domain.Tests.Test Test { get; set; }

	public required FinishedTestAttempt TestAttempt { get; set; }

	public required IEnumerable<QuestionResult> QuestionResults { get; set; }

	public string TestFeedback { get; set; } = string.Empty;
}

public class DbRefereeEmailFeedbackContextFactory
{
	private readonly IQueryable<Models.Data.TestAttempt> testAttempts;
	private readonly IQueryable<TestResult> testResults;
	private readonly ITestContextProvider testContextProvider;
	private readonly ILogger<DbRefereeEmailFeedbackContextFactory> logger;

	public DbRefereeEmailFeedbackContextFactory(
		IQueryable<Models.Data.TestAttempt> testAttempts,
		IQueryable<TestResult> testResults,
		ITestContextProvider testContextProvider,
		ILogger<DbRefereeEmailFeedbackContextFactory> logger)
	{
		this.testAttempts = testAttempts;
		this.testResults = testResults;
		this.testContextProvider = testContextProvider;
		this.logger = logger;
	}

	public async Task<DbRefereeEmailFeedbackContext> LoadAsync(TestAttemptIdentifier testAttemptId, CancellationToken cancellationToken)
	{
		this.logger.LogInformation("Loading referee email feedback context for test attempt ({testAttemptId}).", testAttemptId);

		var attemptWrapper = await this.testResults.AsNoTracking()
			.Where(tr => tr.UniqueId == testAttemptId.ToString())
			.Include(tr => tr.Referee)
			.Include(tr => tr.Test).ThenInclude(t => t!.Certification)
			.Select(tr => new
			{
				Attempt = new FinishedTestAttempt
				{
					AwardedCertifications = DbRefereeTestContextFactory.GetAwardedCertifications(tr.Test!.Certification!, tr.Test.Recertification ?? false),
					FinishedAt = tr.CreatedAt,
					FinishMethod = TestAttemptFinishMethod.Submission,
					Level = tr.Test!.Certification!.Level,
					Version = tr.Test!.Certification!.Version!.Value,
					PassPercentage = tr.MinimumPassPercentage ?? default,
					Passed = tr.Passed ?? false,
					Score = tr.Percentage ?? default,
					StartedAt = tr.CreatedAt - TimeSpan.Parse(tr.Duration ?? "00:00:00"),
					TestId = tr.Test.UniqueId != null ? TestIdentifier.Parse(tr.Test.UniqueId) : TestIdentifier.FromLegacyTestId(tr.Test.Id),
					UserId = tr.Referee.UniqueId != null ? UserIdentifier.Parse(tr.Referee.UniqueId) : UserIdentifier.FromLegacyUserId(tr.Referee.Id),
					Id = testAttemptId,
					IsRecertification = tr.Test.Recertification ?? false,
				},
				TestFeedback = tr.Passed == true ? tr.Test.PositiveFeedback : tr.Test.NegativeFeedback,
			})
			.SingleOrDefaultAsync(cancellationToken);

		if (attemptWrapper is null)
		{
			throw new NotFoundException(testAttemptId.ToString());
		}

		var attempt = attemptWrapper.Attempt;
		var testFeedback = attemptWrapper.TestFeedback;

		var test = await this.testContextProvider.GetTestAsync(attempt.UserId, attempt.TestId, cancellationToken);

		var questionResults = await this.testAttempts.AsNoTracking()
			.Where(ta => ta.UniqueId == testAttemptId.ToString())
			.Include(ta => ta.RefereeAnswers).ThenInclude(ta => ta.Question)
			.Include(ta => ta.RefereeAnswers).ThenInclude(ta => ta.Answer)
			.SelectMany(ta => ta.RefereeAnswers)
			.Select(ra => new QuestionResult
			{
				AnsweredCorrectly = ra.Answer.Correct,
				HtmlText = ra.Question.Description,
				Feedback = ra.Question.Feedback,
				Points = ra.Question.PointsAvailable,
			})
			.ToListAsync(cancellationToken);

		return new DbRefereeEmailFeedbackContext
		{
			QuestionResults = questionResults,
			Test = test,
			TestAttempt = attempt,
			TestFeedback = testFeedback ?? string.Empty,
		};
	}
}
