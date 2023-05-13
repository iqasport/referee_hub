using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands.Tests;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Storage.Commands.Referee;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Tests;
public class SaveSubmittedTestCommand : ISaveSubmittedTestCommand
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<SaveSubmittedTestCommand> logger;
	private readonly IDatabaseTransactionProvider transactionProvider;
	private readonly ISystemClock clock;

	public SaveSubmittedTestCommand(ManagementHubDbContext dbContext, ILogger<SaveSubmittedTestCommand> logger, IDatabaseTransactionProvider transactionProvider, ISystemClock clock)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.transactionProvider = transactionProvider;
		this.clock = clock;
	}

	public async Task SaveSubmittedTestAsync(FinishedTestAttempt finishedTest, IEnumerable<(QuestionId questionId, AnswerId answerId)> markedQuestions)
	{
		this.logger.LogInformation(0, "Saving submitted test attempt for referee ({userId}) and test ({testId})", finishedTest.UserId, finishedTest.TestId);

		await using var transaction = await this.transactionProvider.BeginAsync();

		var userId = await this.dbContext.Users.AsNoTracking().WithIdentifier(finishedTest.UserId).Select(u => u.Id).SingleAsync();
		var testId = await this.dbContext.Tests.AsNoTracking().WithIdentifier(finishedTest.TestId).Select(t => t.Id).SingleAsync();

		var attempt = new Models.Data.TestAttempt
		{
			UniqueId = finishedTest.Id.ToString(),
			CreatedAt = this.clock.UtcNow.UtcDateTime,
			NextAttemptAt = null,
			UpdatedAt = this.clock.UtcNow.UtcDateTime,
			RefereeId = userId,
			TestId = testId,
			TestLevel = finishedTest.Level.ToTestLevel(),
			RefereeAnswers = markedQuestions.Select(p => new RefereeAnswer
			{
				AnswerId = p.answerId.Id,
				CreatedAt = this.clock.UtcNow.UtcDateTime,
				QuestionId = p.questionId.Id,
				RefereeId = userId,
				TestId = testId,
				UpdatedAt = this.clock.UtcNow.UtcDateTime,
			}).ToArray(),
		};

		var result = new Models.Data.TestResult
		{
			UniqueId = finishedTest.Id.ToString(),
			CreatedAt = finishedTest.FinishedAt, // IMPORTANT
			UpdatedAt = this.clock.UtcNow.UtcDateTime,
			Duration = finishedTest.Duration.ToString("hh\\:mm\\:ss"),
			MinimumPassPercentage = (int)finishedTest.PassPercentage,
			Passed = finishedTest.Passed,
			Percentage = (int)finishedTest.Score,
			RefereeId = userId,
			TestId = testId,
			TestLevel = finishedTest.Level.ToTestLevel(),
			TimeFinished = TimeOnly.FromDateTime(finishedTest.FinishedAt),
			TimeStarted = TimeOnly.FromDateTime(finishedTest.StartedAt),
		};

		this.dbContext.TestAttempts.Add(attempt);
		this.dbContext.TestResults.Add(result);

		await this.dbContext.SaveChangesAsync();
		await transaction.CommitAsync();
	}
}
