using System;
using ManagementHub.Models.Enums;

namespace ManagementHub.Storage.Extensions;
public static class EnumExtensions
{
	public static TestLevel ToTestLevel(this CertificationLevel certificationLevel) => certificationLevel switch
	{
		CertificationLevel.Flag => TestLevel.Snitch,
		CertificationLevel.Assistant => TestLevel.Assistant,
		CertificationLevel.Head => TestLevel.Head,
		CertificationLevel.Scorekeeper => TestLevel.Scorekeeper,
		_ => throw new NotSupportedException($"Cannot convert '{certificationLevel}' to test level.")
	};
}
