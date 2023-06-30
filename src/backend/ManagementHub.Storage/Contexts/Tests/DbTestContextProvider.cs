using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.Tests.Policies;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Tests;
public class DbTestContextProvider : ITestContextProvider
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<DbTestContextProvider> logger;

	public DbTestContextProvider(ManagementHubDbContext dbContext, ILogger<DbTestContextProvider> logger)
	{
		this.dbContext = dbContext;
		this.logger = logger;
	}

	public async Task<Test> GetTestAsync(TestIdentifier testId, CancellationToken cancellationToken)
	{
		var test = await Query(this.dbContext.Tests.WithIdentifier(testId), withQuestions: false).SingleOrDefaultAsync(cancellationToken);

		if (test == null)
		{
			throw new NotFoundException($"Could not find test ({testId}).");
		}

		return test;
	}

	public async Task<IEnumerable<Test>> GetTestsAsync(CancellationToken cancellationToken)
	{
		return await Query(this.dbContext.Tests, withQuestions: false).ToListAsync(cancellationToken);
	}

	public async Task<TestWithQuestions> GetTestWithQuestionsAsync(TestIdentifier testId, CancellationToken cancellationToken)
	{
		var test = await Query(this.dbContext.Tests.AsNoTracking().WithIdentifier(testId), withQuestions: true).SingleOrDefaultAsync(cancellationToken);

		if (test == null)
		{
			throw new NotFoundException($"Could not find test ({testId}).");
		}

		if (test is not TestWithQuestions testWithQuestions)
		{
			throw new InvalidOperationException("Expecting test to have questions - bug?");
		}

		return testWithQuestions;
	}

	private static IQueryable<Test> Query(IQueryable<Models.Data.Test> dataset, bool withQuestions)
	{
		dataset = dataset.Include(t => t.Certification).Include(t => t.NewLanguage);
		if (withQuestions)
			dataset = dataset.Include(t => t.Questions).ThenInclude(q => q.Answers);
		return withQuestions
			? dataset.Select(t => new TestWithQuestions
			{
				AvailableQuestions = ConvertQuestions(t.Questions),
				AwardedCertifications = new HashSet<Certification> { Certification.New(t.Certification!.Level, t.Certification.Version) },
				Description = t.Description,
				IsActive = t.Active,
				Language = new LanguageIdentifier(t.NewLanguage!.ShortName, t.NewLanguage.ShortRegion),
				MaximumAttempts = (t.Recertification ?? false) ? 1 : 6,
				Title = t.Name ?? "Unnamed test",
				PassPercentage = t.MinimumPassPercentage,
				QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = t.TestableQuestionCount },
				RecertificationFor = (t.Recertification ?? false) ? Certification.New(t.Certification.Level, t.Certification.Version - 1) : null,
				TestId = t.UniqueId != null ? TestIdentifier.Parse(t.UniqueId) : TestIdentifier.FromLegacyTestId(t.Id),
				TimeLimit = TimeSpan.FromMinutes(t.TimeLimit),
			})
			: dataset.Select(t => new Test
			{
				AwardedCertifications = new HashSet<Certification> { Certification.New(t.Certification!.Level, t.Certification.Version) },
				Description = t.Description,
				IsActive = t.Active,
				Language = new LanguageIdentifier(t.NewLanguage!.ShortName, t.NewLanguage.ShortRegion),
				MaximumAttempts = (t.Recertification ?? false) ? 1 : 6,
				Title = t.Name ?? "Unnamed test",
				PassPercentage = t.MinimumPassPercentage,
				QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = t.TestableQuestionCount },
				RecertificationFor = (t.Recertification ?? false) ? Certification.New(t.Certification.Level, t.Certification.Version - 1) : null,
				TestId = t.UniqueId != null ? TestIdentifier.Parse(t.UniqueId) : TestIdentifier.FromLegacyTestId(t.Id),
				TimeLimit = TimeSpan.FromMinutes(t.TimeLimit),
			});
	}

	private static IEnumerable<Question> ConvertQuestions(ICollection<Models.Data.Question> questions)
	{
		return questions.Select(q =>
		{
			HashSet<Answer> answers = new(4);
			Answer? correctAnswer = null;

			foreach (var dbAnswer in q.Answers)
			{
				var answer = new Answer
				{
					AnswerId = new AnswerId(dbAnswer.Id),
					HtmlText = dbAnswer.Description,
				};
				answers.Add(answer);

				if (dbAnswer.Correct)
				{
					correctAnswer = answer;
				}
			}

			return new Question
			{
				Answers = answers,
				CorrectAnswers = correctAnswer != null ? new HashSet<Answer> { correctAnswer } : new HashSet<Answer>(),
				HtmlText = q.Description,
				Points = q.PointsAvailable,
				QuestionId = new QuestionId(q.Id),
			};
		}).ToList();
	}
}
