using System;
using System.Collections.Generic;
using System.Linq;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Enums;

namespace ManagementHub.Processing.Domain.Tests.Extensions;
public static class CertificationExtensions
{
	public static Certification? Max(this IEnumerable<Certification> certifications) => certifications.Max(CertificationComparer.Instance);
	public static Certification? Min(this IEnumerable<Certification> certifications) => certifications.Min(CertificationComparer.Instance);
	public static IOrderedEnumerable<Certification> Order(this IEnumerable<Certification> certifications) => certifications.Order(CertificationComparer.Instance);
	public static CertificationLevel Max(this IEnumerable<CertificationLevel> levels) => levels.Max(CertificationLevelComparer.Instance);
	public static CertificationLevel Min(this IEnumerable<CertificationLevel> levels) => levels.Min(CertificationLevelComparer.Instance);

	public static int Compare(this CertificationLevel l1, CertificationLevel? l2) => CertificationLevelOrNullComparer.Instance.Compare(l1, l2);
	public static int Compare(this CertificationLevel? l1, CertificationLevel? l2) => CertificationLevelOrNullComparer.Instance.Compare(l1, l2);

	private sealed class CertificationComparer : IComparer<Certification>
	{
		public static CertificationComparer Instance = new CertificationComparer();

		public int Compare(Certification? x, Certification? y)
		{
			if (x == null)
			{
				if (y == null)
					return 0;
				return -1;
			}
			if (y == null)
				return 1;
			if (x.Version == y.Version)
				return CertificationLevelComparer.Instance.Compare(x.Level, y.Level);
			return x.Version.CompareTo(y.Version);
		}
	}

	private sealed class CertificationLevelOrNullComparer : IComparer<CertificationLevel?>
	{
		public static CertificationLevelOrNullComparer Instance { get; } = new CertificationLevelOrNullComparer();

		public int Compare(CertificationLevel? x, CertificationLevel? y)
		{
			if (x == null)
			{
				if (y == null)
					return 0;
				return -1;
			}
			if (y == null)
				return 1;
			return CertificationLevelComparer.Instance.Compare(x.Value, y.Value);
		}
	}

	private sealed class CertificationLevelComparer : IComparer<CertificationLevel>
	{
		public static CertificationLevelComparer Instance { get; } = new CertificationLevelComparer();
		public int Compare(CertificationLevel x, CertificationLevel y) =>
			LevelSortMapper(x).CompareTo(LevelSortMapper(y));

		private static int LevelSortMapper(CertificationLevel level) => level switch
		{
			CertificationLevel.Scorekeeper => 0,
			CertificationLevel.Assistant => 1,
			CertificationLevel.Flag => 2,
			CertificationLevel.Head => 3,
			_ => throw new NotSupportedException(level.ToString())
		};
	}
}
