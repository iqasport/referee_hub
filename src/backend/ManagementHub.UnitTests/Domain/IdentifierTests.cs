using System;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using Xunit;

namespace ManagementHub.UnitTests.Domain;

public class IdentifierTests
{
	[Theory]
	[InlineData(1L, "U_aeaaaaaaaaaaaaaaaaaaaaaaaa")]
	[InlineData(1746L, "U_2idaaaaaaaaaaaaaaaaaaaaaaa")]
	public void UserId_FromLegacy(long id, string expected)
	{
		var userId = UserIdentifier.FromLegacyUserId(id);
		Assert.Equal(expected, userId.ToString());

		var reparsed = UserIdentifier.Parse(userId.ToString());
		Assert.Equal(userId, reparsed);
	}

	[Theory]
	[InlineData("U_svwmdvcr4kaubpjt4is3aldlnm")]
	public void UserId_IsParseIdempotent(string expected) => Assert.Equal(expected, UserIdentifier.Parse(expected).ToString());

	[Theory]
	[InlineData(1L, "T_aeaaaaaaaaaaaaaaaaaaaaaaaa")]
	[InlineData(1746L, "T_2idaaaaaaaaaaaaaaaaaaaaaaa")]
	public void TestId_FromLegacy(long id, string expected)
	{
		var testId = TestIdentifier.FromLegacyTestId(id);
		Assert.Equal(expected, testId.ToString());

		var reparsed = TestIdentifier.Parse(testId.ToString());
		Assert.Equal(testId, reparsed);
	}

	[Theory]
	[InlineData("T_wabkz77ahhtexn2v23krhpuh7a")]
	public void TestId_IsParseIdempotent(string expected) => Assert.Equal(expected, TestIdentifier.Parse(expected).ToString());

	[Theory]
	[InlineData(1L, "2022-01-01T13:49:55", "TAT_01FRAYETXR0400000000000000")]
	[InlineData(1746L, "2022-01-01T13:49:55", "TAT_01FRAYETXRT830000000000000")]
	public void TestAttemptId_FromLegacy(long id, string timestamp, string expected)
	{
		var date = DateTime.Parse(timestamp, styles: System.Globalization.DateTimeStyles.AssumeUniversal);
		var testAttemptId = TestAttemptIdentifier.FromLegacyId(date, id);
		Assert.Equal(expected, testAttemptId.ToString());

		var reparsed = TestAttemptIdentifier.Parse(testAttemptId.ToString());
		Assert.Equal(testAttemptId, reparsed);
	}

	[Theory]
	[InlineData("TAT_01H0CWDYKBHX3WYZH9YRXNNM7A")]
	public void TestAttemptId_IsParseIdempotent(string expected) => Assert.Equal(expected, TestAttemptIdentifier.Parse(expected).ToString());
}
