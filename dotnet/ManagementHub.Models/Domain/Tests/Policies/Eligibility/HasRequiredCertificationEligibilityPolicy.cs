using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.Tests.Policies.Eligibility;
public class HasRequiredCertificationEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IRefereeContextProvider refereeContextProvider;

	public HasRequiredCertificationEligibilityPolicy(IRefereeContextProvider refereeContextProvider)
	{
		this.refereeContextProvider = refereeContextProvider;
	}

	public async Task<bool> IsUserEligibleForTestAsync(Test test, UserIdentifier userId)
	{
		var referee = await this.refereeContextProvider.GetRefereeTestContextAsync(userId);

		if (test.RecertificationFor != null)
		{
			var previousVersion = test.RecertificationFor.Version;
			var awardedVersion = test.AwardedCertifications.First().Version;
			var previousCertification = referee.AcquiredCertifications
				.Where(c => c.Version == previousVersion)
				.Select(c => c.Level)
				.ToArray();
			var highestPreviousLevel = previousCertification.Length > 0
				? (CertificationLevel?)previousCertification.Max(CertificationLevelComparer.Instance)
				: null;
			return
				// referee has a certification this test recertifies
				referee.AcquiredCertifications.Contains(test.RecertificationFor) &&
				// the recertification level is greater or equal to the highest previously acquired level
				CertificationLevelOrNullComparer.Instance.Compare(test.RecertificationFor.Level, highestPreviousLevel) >= 0 &&
				// referee does not have any other certifications for the version of rulebook this provides
				!referee.AcquiredCertifications.Where(c => c.Version == awardedVersion).Any();
		}

		Certification lowestAwardedCert = test.AwardedCertifications.Min(CertificationComparer.Instance)!;

		Certification? requiredCert = GetRequiredCertificationToAttempt(lowestAwardedCert);

		if (requiredCert != null)
		{
			return referee.AcquiredCertifications.Contains(requiredCert);
		}

		return true;
	}

	private static Certification? GetRequiredCertificationToAttempt(Certification lowestAwardedCert)
	{
		switch (lowestAwardedCert.Level)
		{
			case CertificationLevel.Scorekeeper: return null;
			case CertificationLevel.Assistant: return null;
			case CertificationLevel.Snitch: return new Certification(CertificationLevel.Assistant, lowestAwardedCert.Version);
			case CertificationLevel.Head: return new Certification(CertificationLevel.Snitch, lowestAwardedCert.Version);
			default: throw new InvalidOperationException("Could not determine required certification.");
		}
	}

	private static Certification GetLowestAwardedCertification(IEnumerable<Certification> awardedCertifications)
	{
		var scorekeeper = awardedCertifications.FirstOrDefault(static c => c.Level == CertificationLevel.Scorekeeper);
		if (scorekeeper != default)
		{
			return scorekeeper;
		}

		var assistant = awardedCertifications.FirstOrDefault(static c => c.Level == CertificationLevel.Assistant);
		if (assistant != default)
		{
			return assistant;
		}

		var flag = awardedCertifications.FirstOrDefault(static c => c.Level == CertificationLevel.Snitch);
		if (flag != default)
		{
			return flag;
		}

		var head = awardedCertifications.FirstOrDefault(static c => c.Level == CertificationLevel.Head);
		if (head != default)
		{
			return head;
		}

		throw new InvalidOperationException("Could not find the lowest awarded certification.");
	}

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
