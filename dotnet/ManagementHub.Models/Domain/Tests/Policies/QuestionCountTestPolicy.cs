using System;

namespace ManagementHub.Models.Domain.Tests.Policies;

/// <summary>
/// Policy describing how many questions should be issued for a test.
/// </summary>
public class QuestionCountTestPolicy : ITestPolicy
{
	public QuestionCountTestPolicy(int questionCount)
	{
		if (questionCount <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(questionCount));
		}

		this.QuestionCount = questionCount;
	}

	public int QuestionCount { get; }
}
