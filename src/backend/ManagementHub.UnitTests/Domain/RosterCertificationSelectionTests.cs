using System;
using System.Linq;
using FluentAssertions;
using ManagementHub.Models.Data;
using ManagementHub.Models.Enums;
using ManagementHub.Storage;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ManagementHub.UnitTests.Domain;

public class RosterCertificationSelectionTests
{
	[Theory]
	[InlineData(CertificationLevel.Scorekeeper, CertificationVersion.TwentyFour, CertificationLevel.FlagRunner, CertificationVersion.TwentyFour)]
	[InlineData(CertificationLevel.FlagRunner, CertificationVersion.TwentyFour, CertificationLevel.Assistant, CertificationVersion.TwentyFour)]
	[InlineData(CertificationLevel.Assistant, CertificationVersion.TwentyFour, CertificationLevel.Flag, CertificationVersion.TwentyFour)]
	[InlineData(CertificationLevel.Flag, CertificationVersion.TwentyFour, CertificationLevel.Head, CertificationVersion.TwentyFour)]
	[InlineData(CertificationLevel.Head, CertificationVersion.TwentyFour, CertificationLevel.Field, CertificationVersion.TwentyFour)]
	[InlineData(CertificationLevel.Field, CertificationVersion.TwentyTwo, CertificationLevel.Scorekeeper, CertificationVersion.TwentyFour)]
	[InlineData(CertificationLevel.Field, CertificationVersion.Twenty, CertificationLevel.Assistant, CertificationVersion.TwentyTwo)]
	[InlineData(CertificationLevel.Head, CertificationVersion.TwentyTwo, CertificationLevel.Head, CertificationVersion.TwentyFour)]
	[InlineData(CertificationLevel.Field, null, CertificationLevel.Scorekeeper, CertificationVersion.Eighteen)]
	public void OrderForRosterDisplay_ShouldPreferReleaseThenLevel(
		CertificationLevel otherLevel,
		CertificationVersion? otherVersion,
		CertificationLevel expectedLevel,
		CertificationVersion expectedVersion)
	{
		var expected = CreateCertification(expectedLevel, expectedVersion);
		var other = CreateCertification(otherLevel, otherVersion);
		other.ReceivedAt = expected.ReceivedAt!.Value.AddDays(10);

		foreach (var input in new[] { new[] { other, expected }, new[] { expected, other } })
		{
			input.AsQueryable().OrderForRosterDisplay().First().Should().BeSameAs(expected);
		}
	}

	[Fact]
	public void OrderForRosterDisplay_ShouldBreakTiesByReceivedDate()
	{
		var older = CreateCertification(CertificationLevel.Head, CertificationVersion.TwentyFour);
		var newer = CreateCertification(CertificationLevel.Head, CertificationVersion.TwentyFour);
		newer.ReceivedAt = older.ReceivedAt!.Value.AddDays(1);

		new[] { older, newer }.AsQueryable().OrderForRosterDisplay().First().Should().BeSameAs(newer);
	}

	[Fact]
	public void OrderForRosterDisplay_ShouldExcludeRevokedAndUnreceivedCertifications()
	{
		var received = CreateCertification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo);
		var revoked = CreateCertification(CertificationLevel.Field, CertificationVersion.TwentyFour);
		revoked.RevokedAt = revoked.ReceivedAt;
		var unreceived = CreateCertification(CertificationLevel.Head, CertificationVersion.TwentyFour);
		unreceived.ReceivedAt = null;

		new[] { revoked, unreceived, received }.AsQueryable().OrderForRosterDisplay()
			.Should().ContainSingle().Which.Should().BeSameAs(received);
	}

	[Fact]
	public void OrderForRosterDisplay_WithoutEligibleCertifications_ShouldReturnEmpty()
	{
		Array.Empty<RefereeCertification>().AsQueryable().OrderForRosterDisplay().Should().BeEmpty();
		var revoked = CreateCertification(CertificationLevel.Head, CertificationVersion.TwentyFour);
		revoked.RevokedAt = revoked.ReceivedAt;
		new[] { revoked }.AsQueryable().OrderForRosterDisplay().Should().BeEmpty();
	}

	[Fact]
	public void OrderForRosterDisplay_OnlyUnversionedCertifications_ShouldStillRankLevels()
	{
		var head = CreateCertification(CertificationLevel.Head, null);
		var scorekeeper = CreateCertification(CertificationLevel.Scorekeeper, null);

		new[] { scorekeeper, head }.AsQueryable().OrderForRosterDisplay().First().Should().BeSameAs(head);
	}

	[Fact]
	public void OrderForRosterDisplay_ShouldTranslateCorrelatedSelectionToPostgresSql()
	{
		var options = new DbContextOptionsBuilder<ManagementHubDbContext>()
			.UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
			.Options;
		using var db = new ManagementHubDbContext(options);
		var certifications = db.RefereeCertifications.OrderForRosterDisplay();
		var query = db.TournamentTeamParticipants.SelectMany(participant => participant.RosterEntries)
			.Select(entry => new
			{
				entry.UserId,
				Certifications = certifications.Where(rc => rc.RefereeId == entry.UserId)
					.Select(rc => new { rc.Certification.DisplayName, rc.ReceivedAt })
					.Take(1).ToList()
			});

		var sql = query.ToQueryString();
		sql.Should().Contain("ORDER BY");
		sql.Should().Contain("version");
		sql.Should().Contain("received_at");
		sql.Should().Contain("revoked_at");
	}

	private static RefereeCertification CreateCertification(CertificationLevel level, CertificationVersion? version) =>
		new()
		{
			Certification = new Certification { Level = level, Version = version, DisplayName = $"{level} {version}" },
			ReceivedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
		};
}
