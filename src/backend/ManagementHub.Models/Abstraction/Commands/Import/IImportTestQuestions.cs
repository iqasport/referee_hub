using System.Collections.Generic;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Models.Abstraction.Commands.Import;

public interface IImportTestQuestions
{
	Task ImportTestQuestionsAsync(TestIdentifier testId, IEnumerable<TestQuestionRecord> questions);
}

public class TestQuestionRecord
{
	public required int SequenceNum { get; set; }
	public required string Question { get; set; }
	public string? Feedback { get; set; }
	public required string Answer1 { get; set; }
	public required string Answer2 { get; set; }
	public required string Answer3 { get; set; }
	public required string Answer4 { get; set; }
	// Which answer is correct
	public required int Correct { get; set; }
}
