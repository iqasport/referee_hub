using System.Collections.Generic;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface IRefereeTestContext
{
	/// <summary>
	/// User ID of the referee.
	/// </summary>
	UserIdentifier UserId { get; }

	/// <summary>
	/// Certifications acquired by this referee.
	/// </summary>
	HashSet<Certification> AcquiredCertifications { get; }

	/// <summary>
	/// Test attempts by this referee.
	/// </summary>
	IEnumerable<TestAttempt> TestAttempts { get; }

	/// <summary>
	/// Set of certification versions for which the referee has paid the fee for to attempt the Head Referee test.
	/// </summary>
	IEnumerable<CertificationVersion> HeadCertificationsPaid { get; }
}
