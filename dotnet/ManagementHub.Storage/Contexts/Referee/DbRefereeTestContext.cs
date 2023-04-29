using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.Referee;
using User = ManagementHub.Models.Data.User;

public class DbRefereeTestContext : IRefereeTestContext
{
	public required UserIdentifier UserId { get; set; }

	public required HashSet<Models.Domain.Tests.Certification> AcquiredCertifications { get; set; }

	public required IEnumerable<Models.Domain.Tests.TestAttempt> TestAttempts { get; set; }

	public required IEnumerable<CertificationVersion> HeadCertificationsPaid { get; set; }
}

public class DbRefereeTestContextFactory
{
	private readonly IQueryable<User> users;
	private readonly IQueryable<Certification> certifications;
	private readonly IQueryable<RefereeCertification> refereeCertifications;
	private readonly IQueryable<TestResult> testResults;
	private readonly IQueryable<CertificationPayment> certificationPayment;
	private readonly ILogger<DbRefereeTestContextFactory> logger;

	public DbRefereeTestContextFactory(
		IQueryable<User> users,
		IQueryable<Certification> certifications,
		IQueryable<RefereeCertification> refereeCertifications,
		IQueryable<TestResult> testResults,
		IQueryable<CertificationPayment> certificationPayment,
		ILogger<DbRefereeTestContextFactory> logger)
	{
		this.users = users;
		this.certifications = certifications;
		this.refereeCertifications = refereeCertifications;
		this.testResults = testResults;
		this.certificationPayment = certificationPayment;
		this.logger = logger;
	}

	public async Task<DbRefereeTestContext> LoadAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0, "Loading referee test context for user ({userId}).", userId);
		var testResultsSource = this.testResults
			.Include(tr => tr.Test)
			.Join(this.certifications, tr => tr.Test.CertificationId, c => c.Id, (tr, c) => new { TestResult = tr, Certification = c });
		var referee = await this.users.WithIdentifier(userId)
			.GroupJoin(this.refereeCertifications.Include(c => c.Certification), u => u.Id, c => c.RefereeId, (u, c) => new { User = u, Certifications = c })
			.SelectMany(join => join.Certifications.DefaultIfEmpty(), (join, c) => new { join.User, Certification = c })
			.GroupJoin(this.certificationPayment.Include(c => c.Certification), join => join.User.Id, p => p.UserId, (join, p) => new { join.User, join.Certification, Payments = p})
			.SelectMany(join => join.Payments.DefaultIfEmpty(), (join, p) => new { join.User, join.Certification, Payment = p })
			.GroupJoin(testResultsSource, join => join.User.Id, tr => tr.TestResult.RefereeId, (join, tr) => new {Rest = join, TestResults = tr})
			.SelectMany(join => join.TestResults.DefaultIfEmpty(), (join, tr) => new { join.Rest.User, join.Rest.Certification, join.Rest.Payment, TestResult = tr})
			.GroupBy(join => join.User.Id, (uId, g) => new
			{
				UserId = uId,
				Certifications = g.Where(gg => gg.Certification != null).Select(gg => gg.Certification),
				Payments = g.Where(gg => gg.Payment != null).Select(gg => gg.Payment),
				TestResuls = g.Where(gg => gg.TestResult != null).Select(gg => gg.TestResult),
			})
			.Select(r => new DbRefereeTestContext
			{
				UserId = userId,
				AcquiredCertifications = r.Certifications.Select(rc => new Models.Domain.Tests.Certification(rc.Certification.Level, rc.Certification.Version)).ToHashSet(),
				HeadCertificationsPaid = r.Payments.Select(p => p.Certification.Version ?? default).ToList(),
				TestAttempts = r.TestResuls.Select(tr => new Models.Domain.Tests.FinishedTestAttempt
				{
					// TODO: recert tests can award multiple certs
					AwardedCertifications = new HashSet<Models.Domain.Tests.Certification> { new Models.Domain.Tests.Certification(tr.Certification.Level, tr.Certification.Version) },
					FinishedAt = tr.TestResult.CreatedAt,
					FinishMethod = Models.Domain.Tests.TestAttemptFinishMethod.Submission,
					Level = tr.Certification.Level,
					PassPercentage = tr.TestResult.MinimumPassPercentage ?? default,
					Passed = tr.TestResult.Passed ?? false,
					Score = tr.TestResult.Percentage ?? default,
					StartedAt = tr.TestResult.CreatedAt - TimeSpan.Parse(tr.TestResult.Duration ?? "00:00:00"),
					TestId = tr.TestResult.Test.UniqueId != null ? Models.Domain.Tests.TestIdentifier.Parse(tr.TestResult.Test.UniqueId) : Models.Domain.Tests.TestIdentifier.FromLegacyTestId(tr.TestResult.Test.Id),
					UserId = userId,
				})
			})
			.SingleOrDefaultAsync(cancellationToken);

		if (referee == null)
		{
			throw new NotFoundException($"Could not find referee test context for user ({userId}).");
		}

		return referee;
	}
}
