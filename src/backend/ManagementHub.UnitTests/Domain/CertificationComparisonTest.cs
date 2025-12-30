using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Enums;
using ManagementHub.Processing.Domain.Tests.Extensions;
using Xunit;

namespace ManagementHub.UnitTests.Domain;

public class CertificationComparisonTest
{
	private readonly List<Certification> certifications = new List<Certification>()
	{
		new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
		new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
		new Certification(CertificationLevel.Head, CertificationVersion.Twenty),
		new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Twenty),
		new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen),
		new Certification(CertificationLevel.Flag, CertificationVersion.Eighteen),
		new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Eighteen),
		new Certification(CertificationLevel.Head, CertificationVersion.Eighteen),
		new Certification(CertificationLevel.Flag, CertificationVersion.TwentyTwo),
		new Certification(CertificationLevel.Head, CertificationVersion.TwentyTwo),
		new Certification(CertificationLevel.Scorekeeper, CertificationVersion.TwentyTwo),
		new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
	};

	private readonly List<Certification> sortedCertifications = new List<Certification>()
	{
		new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Eighteen),
		new Certification(CertificationLevel.Assistant, CertificationVersion.Eighteen),
		new Certification(CertificationLevel.Flag, CertificationVersion.Eighteen),
		new Certification(CertificationLevel.Head, CertificationVersion.Eighteen),
		new Certification(CertificationLevel.Scorekeeper, CertificationVersion.Twenty),
		new Certification(CertificationLevel.Assistant, CertificationVersion.Twenty),
		new Certification(CertificationLevel.Flag, CertificationVersion.Twenty),
		new Certification(CertificationLevel.Head, CertificationVersion.Twenty),
		new Certification(CertificationLevel.Scorekeeper, CertificationVersion.TwentyTwo),
		new Certification(CertificationLevel.Assistant, CertificationVersion.TwentyTwo),
		new Certification(CertificationLevel.Flag, CertificationVersion.TwentyTwo),
		new Certification(CertificationLevel.Head, CertificationVersion.TwentyTwo),
	};

	[Fact]
	public void Order_UsingCertificationComparer_SortsThemCorrectly()
	{
		var sorted = this.certifications.Order().ToList();
		for (int i = 0; i < sorted.Count; i++)
		{
			Assert.Equal(this.sortedCertifications[i], sorted[i]);
		}
	}
}
