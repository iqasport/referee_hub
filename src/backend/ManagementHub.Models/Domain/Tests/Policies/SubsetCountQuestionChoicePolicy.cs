using System.Collections.Generic;
using System.Linq;
using ManagementHub.Models.Misc;

namespace ManagementHub.Models.Domain.Tests.Policies;
public class SubsetCountQuestionChoicePolicy : IQuestionChoicePolicy
{
	public required int QuestionsCount { get; set; }

	public IEnumerable<Question> ChooseQuestions(IEnumerable<Question> questions)
	{
		return questions.Shuffle().Take(this.QuestionsCount);
	}
}
