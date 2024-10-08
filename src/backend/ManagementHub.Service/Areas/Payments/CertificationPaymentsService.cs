﻿using System.Text.RegularExpressions;
using Hangfire;
using ManagementHub.Models.Abstraction.Commands.Payments;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Enums;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Extensions;
using Stripe;

namespace ManagementHub.Service.Areas.Payments;

public partial class CertificationPaymentsService : PaymentsService<Certification>
{
	[GeneratedRegex(@"Head Referee Exam Fee \(Rulebook (\d\d\d\d)-.*")]
	private static partial Regex HeadRefCertificationProductName();

	private readonly ILogger<CertificationPaymentsService> logger;
	private readonly IBackgroundJobClient backgroundJobClient;
	private readonly IRefereeContextAccessor refereeContextAccessor;

	public CertificationPaymentsService(
		IStripeClient stripeClient,
		ILogger<CertificationPaymentsService> logger,
		IBackgroundJobClient backgroundJobClient,
		IRefereeContextAccessor refereeContextAccessor)
		: base(stripeClient, logger)
	{
		this.logger = logger;
		this.backgroundJobClient = backgroundJobClient;
		this.refereeContextAccessor = refereeContextAccessor;
	}

	protected override async Task<IDictionary<Certification, ProductStatus>> CanPurchaseItemsAsync(IEnumerable<Certification> item)
	{
		// TODO: make another smaller context for this check
		var referee = await this.refereeContextAccessor.GetRefereeTestContextForCurrentUserAsync();

		ProductStatus HasPaidForCertification(Certification i)
		{
			if (referee.HeadCertificationsPaid.Contains(i.Version))
			{
				return ProductStatus.AlreadyPurchased;
			}

			return ProductStatus.Available;
		}

		return item.ToDictionary(i => i, HasPaidForCertification);
	}

	protected override Dictionary<string, string> ItemAsMetadata(Certification item)
	{
		return new Dictionary<string, string>
		{
			[nameof(item.Level)] = item.Level.ToString(),
			[nameof(item.Version)] = item.Version.ToString(),
		};
	}

	protected override Certification ItemOfMetadata(Dictionary<string, string> metadata)
	{
		if (!metadata.TryGetValue(nameof(Certification.Level), out string? levelString))
		{
			this.logger.LogError(-0x2a709300, "No value in metadata for level.");
			throw new InvalidOperationException();
		}

		if (!metadata.TryGetValue(nameof(Certification.Version), out string? versionString))
		{
			this.logger.LogError(-0x2a7092ff, "No value in metadata for version.");
			throw new InvalidOperationException();
		}

		if (!Enum.TryParse<CertificationLevel>(levelString, out var level))
		{
			this.logger.LogError(-0x2a7092fe, "Invalid value for level. Got '{value}'.", levelString);
			throw new InvalidOperationException();
		}

		if (!Enum.TryParse<CertificationVersion>(versionString, out var version))
		{
			this.logger.LogError(-0x2a7092fd, "Invalid value for version. Got '{value}'.", versionString);
			throw new InvalidOperationException();
		}

		return new Certification(level, version);
	}

	protected override Certification? ItemOfProduct(Product product)
	{
		var matches = HeadRefCertificationProductName().Match(product.Name);
		if (matches.Success)
		{
			switch (matches.Groups[1].Value) // 0th group represents whole regex, 1st the capture group
			{
				case "2018": return new Certification(CertificationLevel.Head, CertificationVersion.Eighteen);
				case "2020": return new Certification(CertificationLevel.Head, CertificationVersion.Twenty);
				case "2022": return new Certification(CertificationLevel.Head, CertificationVersion.TwentyTwo);
				case "2024": return new Certification(CertificationLevel.Head, CertificationVersion.TwentyFour);
				default:
					this.logger.LogWarning(-0x2a7092fc, "A new certification product has been added to Stripe without a corresponding rulebook version being added to the service.");
					return null;
			}
		}

		return null;
	}

	protected override void ScheduleCompletedSessionProcessing(string sessionId, string userEmail, Certification item)
	{
		this.backgroundJobClient.Enqueue<IProcessCertificationPaymentCommand>(this.logger, p => p.ProcessCertificationPaymentAsync(sessionId, userEmail, item, CancellationToken.None));
	}
}
