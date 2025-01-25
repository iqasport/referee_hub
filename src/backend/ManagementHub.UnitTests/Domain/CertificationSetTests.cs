using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Enums;
using ManagementHub.Models.Misc;
using Xunit;

namespace ManagementHub.UnitTests.Domain;
public class CertificationSetTests
{
	[Fact]
	public void Add_AddsCertification()
	{
		var set = new CertificationSet();
		var certification = new Certification(CertificationLevel.Head, CertificationVersion.TwentyFour);
		set.Add(certification);
		Assert.Contains(certification, set);
	}

	[Fact]
	public void Enumerate_ReturnsAllAddedCertifications()
	{
		var set = new CertificationSet
		{
			new Certification(CertificationLevel.Head, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
			new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Twenty)
		};
		Assert.Equal(3, set.Count);
		
		var hashSet = new HashSet<Certification>(set)
		{
			new Certification(CertificationLevel.Head, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
			new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Twenty)
		};
		Assert.True(hashSet.SetEquals(set));
	}

	[Fact]
	public void Enumerate_ReturnItemsInExpectedOrder()
	{
		var set = new CertificationSet
		{
			new Certification(CertificationLevel.Head, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Flag, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
			new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Twenty)
		};
		var certifications = new List<Certification>()
		{
			new Certification(CertificationLevel.Head, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Flag, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
			new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Twenty)
		};

		var enumerator = set.GetEnumerator();
		for (int i = 0; i < set.Count; i++)
		{
			Assert.True(enumerator.MoveNext());
			Assert.Equal(certifications[i], enumerator.Current);
		}
	}

	[Fact]
	public void First_ReturnsHighestCertification()
	{
		var set = new CertificationSet
		{
			new Certification(CertificationLevel.Head, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Flag, CertificationVersion.TwentyFour),
			new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyFour),
		};
		Assert.Equal(new Certification(CertificationLevel.Head, CertificationVersion.TwentyFour), set.First());
	}
}
