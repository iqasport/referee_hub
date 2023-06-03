using System.Collections.Generic;

namespace ManagementHub.Models.Domain.Tests.Policies;

/// <summary>
/// A policy determining which questions to choose from the available set of questions.
/// </summary>
public interface IQuestionChoicePolicy
{
	IEnumerable<Question> ChooseQuestions(IEnumerable<Question> questions);
}
