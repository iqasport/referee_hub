using System;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Tests;

/// <summary>
/// Certification describes the skill level and rulebook version of a referee's knowledge.
/// </summary>
public record class Certification(CertificationLevel Level, CertificationVersion Version)
{
	public Certification(CertificationLevel level, CertificationVersion? version)
		: this(level, version ?? throw new ArgumentNullException(nameof(version)))
	{
	}
}
