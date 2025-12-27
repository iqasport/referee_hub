using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;

public interface ITestContextProvider
{
	Task<IEnumerable<Test>> GetTestsAsync(UserIdentifier userId, CancellationToken cancellationToken);

	Task<Test> GetTestAsync(UserIdentifier userId, TestIdentifier testId, CancellationToken cancellationToken);

	Task<TestWithQuestions> GetTestWithQuestionsAsync(UserIdentifier userId, TestIdentifier testId, CancellationToken cancellationToken);
}
