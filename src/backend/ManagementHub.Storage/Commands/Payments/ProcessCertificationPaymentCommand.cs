using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands.Payments;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Misc;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Payments;
public class ProcessCertificationPaymentCommand : IProcessCertificationPaymentCommand
{
	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<ProcessCertificationPaymentCommand> logger;
	private readonly IDatabaseTransactionProvider transactionProvider;

	public ProcessCertificationPaymentCommand(ManagementHubDbContext dbContext, ILogger<ProcessCertificationPaymentCommand> logger, IDatabaseTransactionProvider transactionProvider)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.transactionProvider = transactionProvider;
	}

	public async Task ProcessCertificationPaymentAsync(string sessionId, [SensitiveData] string userEmail, Certification certification, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0x2a4cae00, "Processing certification payment for session ({sessionId}) and certification ({certification}).", sessionId, certification);

		try
		{
			await this.ProcessInternalAsync(sessionId, userEmail, certification, cancellationToken);
			this.logger.LogInformation(0x2a4cae01, "Processing certification payment completed successfully.");
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x2a4cae02, ex, "Error occurred while processing certification payment.");
			throw;
		}
	}

	private async Task ProcessInternalAsync(string sessionId, string userEmail, Certification certification, CancellationToken cancellationToken)
	{
		using var transaction = await this.transactionProvider.BeginAsync(IsolationLevel.Serializable);

		var existingPayment = await this.dbContext.Users.WithEmail(userEmail)
			.Include(u => u.CertificationPayments).ThenInclude(p => p.Certification)
			.SelectMany(u => u.CertificationPayments)
			.Where(c => c.Certification.Version == certification.Version && c.Certification.Level == certification.Level)
			.SingleOrDefaultAsync(cancellationToken);

		if (existingPayment is not null)
		{
			this.logger.LogInformation(0x2a4cae03, "A payment record already exists for certification ({certification}).", certification);
			if (existingPayment.StripeSessionId != sessionId)
			{
				this.logger.LogWarning(0x2a4cae04, "Existing payment session ID ({existingSesionId}) is not the same as current one ({sessionId}).", existingPayment.StripeSessionId, sessionId);
			}

			return;
		}

		var userId = await this.dbContext.Users.WithEmail(userEmail)
			.Select(u => u.Id)
			.SingleAsync(cancellationToken);
		var certificationId = await this.dbContext.Certifications
			.Where(c => c.Version == certification.Version && c.Level == certification.Level)
			.Select(c => c.Id)
			.SingleAsync(cancellationToken);

		this.dbContext.CertificationPayments.Add(new Models.Data.CertificationPayment
		{
			CertificationId = certificationId,
			CreatedAt = DateTime.UtcNow,
			StripeSessionId = sessionId,
			UpdatedAt = DateTime.UtcNow,
			UserId = userId,
		});

		await this.dbContext.SaveChangesAsync(cancellationToken);

		await transaction.CommitAsync(cancellationToken);
	}
}
