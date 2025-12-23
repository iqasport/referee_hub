using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Referee;

using User = Models.Data.User;

public class DbRefereeTestContext : IRefereeTestContext
{
	public required UserIdentifier UserId { get; set; }

	public required HashSet<Certification> AcquiredCertifications { get; set; }

	public required IEnumerable<TestAttempt> TestAttempts { get; set; }

	public required IEnumerable<CertificationVersion> HeadCertificationsPaid { get; set; }
}

public class DbRefereeTestContextFactory
{
	private readonly IQueryable<User> users;
	private readonly ILogger<DbRefereeTestContextFactory> logger;

	public DbRefereeTestContextFactory(
		IQueryable<User> users,
		ILogger<DbRefereeTestContextFactory> logger)
	{
		this.users = users;
		this.logger = logger;
	}

	public async Task<DbRefereeTestContext> LoadAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0x5dc5900, "Loading referee test context for user ({userId}).", userId);
		var referee = await this.users.WithIdentifier(userId)
			.Include(u => u.RefereeCertifications).ThenInclude(rc => rc.Certification)
			.Include(u => u.CertificationPayments).ThenInclude(p => p.Certification)
			.Include(u => u.TestResults).ThenInclude(tr => tr.Test).ThenInclude(t => t!.Certification)
			.Select(u => new DbRefereeTestContext
			{
				UserId = userId,
				AcquiredCertifications = u.RefereeCertifications.Select(rc => Certification.New(rc.Certification.Level, rc.Certification.Version)).ToHashSet(),
				HeadCertificationsPaid = u.CertificationPayments.Select(p => p.Certification.Version ?? default).ToList(),
				TestAttempts = u.TestResults.Select(tr => new FinishedTestAttempt
				{
					AwardedCertifications = tr.Passed == true
						? tr.Test != null
							? GetAwardedCertifications(tr.Test.Certification!, tr.Test.Recertification ?? false)
							: tr.TestLevel != null
								? GetAwardedCertifications(new Models.Data.Certification { Level = tr.TestLevel.Value.ToCertificationLevel(), Version = CertificationVersion.Eighteen }, false)
								: new HashSet<Certification>()
						: new HashSet<Certification>(),
					FinishedAt = tr.CreatedAt,
					FinishMethod = TestAttemptFinishMethod.Submission,
					Level = tr.Test != null ? tr.Test.Certification!.Level : tr.TestLevel!.Value.ToCertificationLevel(),
					Version = tr.Test != null ? tr.Test.Certification!.Version!.Value : CertificationVersion.Eighteen,
					PassPercentage = tr.MinimumPassPercentage ?? default,
					Passed = tr.Passed ?? false,
					Score = tr.Percentage ?? default,
					StartedAt = tr.CreatedAt - TimeSpan.Parse(tr.Duration ?? "00:00:00"),
					TestId = tr.Test != null
						? tr.Test.UniqueId != null ? TestIdentifier.Parse(tr.Test.UniqueId) : TestIdentifier.FromLegacyTestId(tr.Test.Id)
						: new TestIdentifier(),
					UserId = userId,
					Id = tr.UniqueId != null ? TestAttemptIdentifier.Parse(tr.UniqueId) : TestAttemptIdentifier.FromLegacyId(tr.CreatedAt, tr.Id),
					IsRecertification = tr.Test != null && tr.Test.Recertification != null ? tr.Test.Recertification.Value : false,
				})
			})
			.SingleOrDefaultAsync(cancellationToken);

		if (referee == null)
		{
			throw new NotFoundException($"Could not find referee test context for user ({userId}).");
		}

		return referee;
	}

	internal static HashSet<Certification> GetAwardedCertifications(Models.Data.Certification testCertification, bool recertification)
	{
		var certifications = new HashSet<Certification>()
		{
			Certification.New(testCertification.Level, testCertification.Version),
		};

		if (recertification)
		{
			if (testCertification.Level == CertificationLevel.Flag)
			{
				certifications.Add(Certification.New(CertificationLevel.Assistant, testCertification.Version));
			}
			else if (testCertification.Level == CertificationLevel.Head)
			{
				certifications.Add(Certification.New(CertificationLevel.Assistant, testCertification.Version));
				certifications.Add(Certification.New(CertificationLevel.Flag, testCertification.Version));
			}
		}

		return certifications;
	}
}
