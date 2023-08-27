using System;
using System.Collections.Generic;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Mailers.Models;

public class FeedbackContextWithHostUrl : IRefereeEmailFeedbackContext
{
	private readonly IRefereeEmailFeedbackContext ctx;
	private readonly Uri baseUrl;

	public FeedbackContextWithHostUrl(IRefereeEmailFeedbackContext ctx, Uri baseUrl)
	{
		this.ctx = ctx;
		this.baseUrl = baseUrl;
	}

	public Test Test => this.ctx.Test;
	public FinishedTestAttempt TestAttempt => this.ctx.TestAttempt;
	public IEnumerable<QuestionResult> QuestionResults => this.ctx.QuestionResults;
	public string TestFeedback => this.ctx.TestFeedback;

	public Uri HostUrl => this.baseUrl;
}
