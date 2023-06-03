using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Models.Abstraction.Contexts.Providers;
public interface ITestContextProvider
{
	Task<IEnumerable<Test>> GetTestsAsync(CancellationToken cancellationToken);

	Task<Test> GetTestAsync(TestIdentifier testId, CancellationToken cancellationToken);

	Task<TestWithQuestions> GetTestWithQuestionsAsync(TestIdentifier testId, CancellationToken cancellationToken);
}
