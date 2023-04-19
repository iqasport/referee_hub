using System;

namespace ManagementHub.Models.Domain.Tests.Policies;

/// <summary>
/// Policy describing the percentage of questions that need to be answered correctly to pass.
/// </summary>
public class PassPercentageTestPolicy : ITestPolicy
{
	public PassPercentageTestPolicy(int percentage)
	{
		if (percentage <= 0 || percentage > 100)
		{
			throw new ArgumentOutOfRangeException(nameof(percentage));
		}

		this.Percentage = percentage;
	}

	public int Percentage { get; }
}
