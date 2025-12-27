using System.Collections.Generic;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Models.Abstraction.Commands.Tests;

public interface ISaveSubmittedTestCommand
{
	Task SaveSubmittedTestAsync(FinishedTestAttempt finishedTest, IEnumerable<(QuestionId questionId, AnswerId answerId)> markedQuestions);
}
