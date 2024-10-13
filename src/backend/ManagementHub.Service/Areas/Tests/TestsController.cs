using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ManagementHub.Models.Abstraction.Commands.Import;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Swagger;
using ManagementHub.Storage;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Areas.Tests;

[Route("api/admin/[controller]")]
[ApiController]
public class TestsController : ControllerBase
{
	private readonly IImportTestQuestions importTestQuestions;
	private readonly ManagementHubDbContext dbContext; // TODO: should I move this to the Storage project?

	public TestsController(IImportTestQuestions importTestQuestions, ManagementHubDbContext dbContext)
	{
		this.importTestQuestions = importTestQuestions;
		this.dbContext = dbContext;
	}

	[HttpPost("create")]
	[Authorize(AuthorizationPolicies.IqaAdminPolicy)] // todo: make it a test admin policy
	public async Task<TestIdentifier> CreateNewTest([FromBody] TestViewModel test)
	{
		var testId = TestIdentifier.NewTestId();
		var language = await this.dbContext.Languages.Where(l => l.ShortName == test.Language.Lang && l.ShortRegion == test.Language.Region).FirstOrDefaultAsync();
		if (language == null)
		{
			throw new NotFoundException(test.Language.ToString());
		}
		var certification = await this.dbContext.Certifications
			.Where(c => c.Level == test.AwardedCertification.Level && c.Version == test.AwardedCertification.Version)
			.FirstOrDefaultAsync();
		if (certification == null)
		{
			throw new NotFoundException(test.AwardedCertification.ToString());
		}

		this.dbContext.Tests.Add(new Models.Data.Test
		{
			UniqueId = testId.ToString(),
			Name = test.Title,
			Description = test.Description,
			NewLanguageId = language.Id,
			CertificationId = certification.Id,
			TimeLimit = test.TimeLimit,
			MinimumPassPercentage = test.PassPercentage,
			TestableQuestionCount = test.QuestionsCount,
			Recertification = test.Recertification,
			NegativeFeedback = test.NegativeFeedback,
			PositiveFeedback = test.PositiveFeedback,
			// A new test should always be inactive until it's ready to be used
			Active = false,

			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		});
		await this.dbContext.SaveChangesAsync();

		return testId;
	}

	[HttpPost("{testId}/active")]
	[Authorize(AuthorizationPolicies.IqaAdminPolicy)] // todo: make it a test admin policy
	public async Task SetTestActive([FromRoute] TestIdentifier testId, [FromBody] bool active)
	{
		var test = await this.dbContext.Tests.WithIdentifier(testId).FirstOrDefaultAsync();
		if (test == null)
		{
			throw new NotFoundException(testId.ToString());
		}

		test.Active = active;
		await this.dbContext.SaveChangesAsync();
	}

	[HttpPost("{testId}/import")]
	[Authorize(AuthorizationPolicies.IqaAdminPolicy)] // todo: make it a test admin policy
	[ExternalParameterInBody("testQuestions", MediaType = "text/csv")]
	public async Task ImportTestQuestions([FromRoute] TestIdentifier testId)
	{
		using (var reader = new StreamReader(this.HttpContext.Request.Body, leaveOpen: true))
		{
			// this function is applied to both CSV headers and the model properties
			PrepareHeaderForMatch columnNameUnifier = args =>
			{
				var columnAliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
				{
					["Question Description"] = nameof(TestQuestionRecord.Question),
					["Answer 1"] = nameof(TestQuestionRecord.Answer1),
					["Answer 2"] = nameof(TestQuestionRecord.Answer2),
					["Answer 3"] = nameof(TestQuestionRecord.Answer3),
					["Answer 4"] = nameof(TestQuestionRecord.Answer4),
					["Correct Answer"] = nameof(TestQuestionRecord.CorrectAnswer),
					["Answer"] = nameof(TestQuestionRecord.CorrectAnswer),
				};

				string header = args.Header;
				if (columnAliases.TryGetValue(header, out var mappedHeader))
				{
					header = mappedHeader;
				}

				return header.ToLower();
			};
			using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = true,
				PrepareHeaderForMatch = columnNameUnifier,
			}))
			{
				csv.Context.RegisterClassMap<TestQuestionRecordMap>();

				var records = await csv.GetRecordsAsync<TestQuestionRecord>().ToListAsync();

				foreach (var record in records)
				{
					if (record.Correct < 1 || record.Correct > 4)
					{
						throw new ArgumentException("Correct answer must be between 1 and 4");
					}

					if (string.IsNullOrWhiteSpace(record.Question) ||
						string.IsNullOrWhiteSpace(record.Answer1) ||
						string.IsNullOrWhiteSpace(record.Answer2) ||
						string.IsNullOrWhiteSpace(record.Answer3) ||
						string.IsNullOrWhiteSpace(record.Answer4))
					{
						throw new ArgumentException($"Question and Answers cannot be empty (sequence: {record.SequenceNum})");
					}

					if ("null".Equals(record.Feedback, StringComparison.OrdinalIgnoreCase))
					{
						record.Feedback = null;
					}
				}

				if (records.Count != records.Select(r => r.SequenceNum).Distinct().Count())
				{
					var duplicates = records.GroupBy(r => r.SequenceNum).Where(g => g.Count() > 1).Select(g => g.Key);
					throw new ArgumentException($"Sequence numbers must be unique (duplicated {string.Join(", ", duplicates)}");
				}

				await this.importTestQuestions.ImportTestQuestionsAsync(testId, records);
			}
		}
	}

	public sealed class TestQuestionRecordMap : ClassMap<TestQuestionRecord>
	{
		public TestQuestionRecordMap()
		{
			this.Map(m => m.SequenceNum);
			this.Map(m => m.Question);
			this.Map(m => m.Feedback).Optional();
			this.Map(m => m.Answer1);
			this.Map(m => m.Answer2);
			this.Map(m => m.Answer3);
			this.Map(m => m.Answer4);
			// Either one of the below must be present
			this.Map(m => m.CorrectAnswer).Optional();
			this.Map(m => m.Correct).Optional();
		}
	}
}
