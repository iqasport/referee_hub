using System;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.Tests.Policies;
using ManagementHub.Models.Enums;

namespace ManagementHub.UnitTests.Tests.Policies.Eligibility;
public static class TestData
{
	public static Test Assistant18 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen) },
		Description = "Mock assitant 18",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "AR18",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
	};

	public static Test Assistant20 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty) },
		Description = "Mock assitant 20",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "AR20",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
	};

	public static Test Assistant22 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo) },
		Description = "Mock assitant 22",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "AR22",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
	};

	public static Test Flag18 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Flag, CertificationVersion.Eighteen) },
		Description = "Mock flag 18",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "FR18",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
	};

	public static Test Flag20 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Flag, CertificationVersion.Twenty) },
		Description = "Mock flag 20",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "FR20",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
	};

	public static Test Flag22 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Flag, CertificationVersion.TwentyTwo) },
		Description = "Mock flag 22",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "FR22",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
	};

	public static Test Head18 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Head, CertificationVersion.Eighteen) },
		Description = "Mock head 18",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "HR18",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 50 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(35),
	};

	public static Test Head20 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Head, CertificationVersion.Twenty) },
		Description = "Mock head 20",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "HR20",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 50 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(35),
	};

	public static Test Head22 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Head, CertificationVersion.TwentyTwo) },
		Description = "Mock head 22",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "HR22",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 50 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(35),
	};

	public static Test Scorekeeper18 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Eighteen) },
		Description = "Mock Scorekeeper 18",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "ScR18",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
	};

	public static Test Scorekeeper20 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Twenty) },
		Description = "Mock Scorekeeper 20",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "ScR20",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
	};

	public static Test Scorekeeper22 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Scorekeeper, CertificationVersion.TwentyTwo) },
		Description = "Mock Scorekeeper 22",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "ScR22",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
	};

	public static Test RecertAssistant22 = new()
	{
		AwardedCertifications = new[] { new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo) },
		Description = "Mock recert assistant 22",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "AR22-r",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
		MaximumAttempts = 1,
		RecertificationFor = new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
	};

	public static Test RecertFlag22 = new()
	{
		AwardedCertifications = new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
			new Certification(CertificationLevel.Flag, CertificationVersion.TwentyTwo),
		},
		Description = "Mock recert flag 22",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "FR22-r",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 25 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(20),
		MaximumAttempts = 1,
		RecertificationFor = new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
	};

	public static Test RecertHead22 = new()
	{
		AwardedCertifications = new[]
		{
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
			new Certification(CertificationLevel.Flag, CertificationVersion.TwentyTwo),
			new Certification(CertificationLevel.Head, CertificationVersion.TwentyTwo),
		},
		Description = "Mock recert head 22",
		IsActive = true,
		Language = LanguageIdentifier.Default,
		Name = "HR22-r",
		PassPercentage = 80,
		QuestionChoicePolicy = new SubsetCountQuestionChoicePolicy { QuestionsCount = 50 },
		TestId = TestIdentifier.NewTestId(),
		TimeLimit = TimeSpan.FromMinutes(35),
		MaximumAttempts = 1,
		RecertificationFor = new Certification(CertificationLevel.Head, CertificationVersion.Twenty),
	};
}
