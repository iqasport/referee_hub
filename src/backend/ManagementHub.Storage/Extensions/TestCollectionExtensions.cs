using System;
using System.Linq;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Enums;

namespace ManagementHub.Storage.Extensions;

public static class TestCollectionExtensions
{
	public static IQueryable<Models.Data.Test> WithIdentifier(this IQueryable<Models.Data.Test> tests, TestIdentifier testId) =>
		tests.Where(test => test.UniqueId == testId.ToString() || (test.UniqueId == null && test.Id == testId.ToLegacyUserId()));

	public static CertificationLevel ToCertificationLevel(this TestLevel testLevel) => testLevel switch
	{
		TestLevel.Snitch => CertificationLevel.Flag,
		TestLevel.Assistant => CertificationLevel.Assistant,
		TestLevel.Head => CertificationLevel.Head,
		TestLevel.Scorekeeper => CertificationLevel.Scorekeeper,
		_ => throw new ArgumentOutOfRangeException(nameof(testLevel), testLevel, null)
	};
}
