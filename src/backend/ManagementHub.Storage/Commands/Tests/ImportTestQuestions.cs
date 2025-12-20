using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands.Import;
using ManagementHub.Models.Data;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Commands.Tests;
internal class ImportTestQuestions : IImportTestQuestions
{
	private readonly ManagementHubDbContext dbContext;

	public ImportTestQuestions(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public async Task ImportTestQuestionsAsync(Models.Domain.Tests.TestIdentifier testId, IEnumerable<TestQuestionRecord> questions)
	{
		// I want to convert the TestQuestionRecord into one instance of Question and 4 instances of Answer
		// The SequenceId needs to match the sequence number of the question
		// Now we need to compare the data to the database such that if a questions for this test by SequenceId doesn't exist we insert it and all the answers
		// but if it does exist we need to update the question and the answers
		using var trx = this.dbContext.Database.BeginTransaction();

		var test = this.dbContext.Tests.WithIdentifier(testId)
			.Include(t => t.Questions).ThenInclude(q => q.Answers)
			.FirstOrDefault();
		if (test == null)
		{
			throw new NotFoundException(testId.ToString());
		}

		var dbQuestions = test.Questions.OrderBy(q => q.SequenceId).ToList();

		int sequenceId = 1;
		foreach (var (qRecord, dbQuestion) in LeftZip(questions, dbQuestions))
		{
			if (dbQuestion == null)
			{
				// Insert
				var question = new Question
				{
					TestId = test.Id,
					SequenceId = sequenceId,
					Description = qRecord.Question,
					PointsAvailable = 1,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow,
					Feedback = qRecord.Feedback,
					Answers = new[]
					{
						new Answer
						{
							Description = qRecord.Answer1,
							Correct = qRecord.Correct == 1,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow,
						},
						new Answer
						{
							Description = qRecord.Answer2,
							Correct = qRecord.Correct == 2,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow,
						},
						new Answer
						{
							Description = qRecord.Answer3,
							Correct = qRecord.Correct == 3,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow,
						},
						new Answer
						{
							Description = qRecord.Answer4,
							Correct = qRecord.Correct == 4,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow,
						}
					}
				};
				this.dbContext.Questions.Add(question);
			}
			else
			{
				// Update
				dbQuestion.Description = qRecord.Question;
				dbQuestion.Feedback = qRecord.Feedback;
				dbQuestion.UpdatedAt = DateTime.UtcNow;
				var dbAnswers = dbQuestion.Answers.OrderBy(a => a.Id).ToList();
				if (dbAnswers.Count != 4)
				{
					throw new InvalidOperationException("Expected 4 answers");
				}

				if (!dbAnswers[0].Description.Equals(qRecord.Answer1, StringComparison.InvariantCulture))
				{
					dbAnswers[0].Description = qRecord.Answer1;
				}
				if (dbAnswers[0].Correct != (qRecord.Correct == 1))
				{
					dbAnswers[0].Correct = qRecord.Correct == 1;
				}

				if (!dbAnswers[1].Description.Equals(qRecord.Answer2, StringComparison.InvariantCulture))
				{
					dbAnswers[1].Description = qRecord.Answer2;
				}
				if (dbAnswers[1].Correct != (qRecord.Correct == 2))
				{
					dbAnswers[1].Correct = qRecord.Correct == 2;
				}

				if (!dbAnswers[2].Description.Equals(qRecord.Answer3, StringComparison.InvariantCulture))
				{
					dbAnswers[2].Description = qRecord.Answer3;
				}
				if (dbAnswers[2].Correct != (qRecord.Correct == 3))
				{
					dbAnswers[2].Correct = qRecord.Correct == 3;
				}

				if (!dbAnswers[3].Description.Equals(qRecord.Answer4, StringComparison.InvariantCulture))
				{
					dbAnswers[3].Description = qRecord.Answer4;
				}
				if (dbAnswers[3].Correct != (qRecord.Correct == 4))
				{
					dbAnswers[3].Correct = qRecord.Correct == 4;
				}
			}

			sequenceId++;
		}

		await this.dbContext.SaveChangesAsync();
		await trx.CommitAsync();
	}

	// TODO: move this out into a utility class and unit test it properly
	// ZIP all elements from the first sequence with the elements from the second sequence padding the right side with null if the second sequence is shorter
	private static IEnumerable<(TFirst First, TSecond? Second)> LeftZip<TFirst, TSecond>(IEnumerable<TFirst> first, IEnumerable<TSecond> second)
	{
		using (IEnumerator<TFirst> e1 = first.GetEnumerator())
		using (IEnumerator<TSecond> e2 = second.GetEnumerator())
		{
			var rightEnd = !e2.MoveNext();
			while (e1.MoveNext())
			{
				if (!rightEnd)
				{
					yield return (e1.Current, e2.Current);
					rightEnd = !e2.MoveNext();
				}
				else
				{
					yield return (e1.Current, default);
				}
			}
		}
	}
}
