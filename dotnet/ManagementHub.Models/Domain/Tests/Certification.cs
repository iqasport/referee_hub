using System;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Tests;
public record struct Certification(CertificationLevel Level, CertificationVersion Version)
{
	public Certification(CertificationLevel level, CertificationVersion? version)
		: this(level, version ?? throw new ArgumentNullException(nameof(version)))
	{
	}
}
