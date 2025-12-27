using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Storage.Attachments;
using ManagementHub.Storage.Database.Transactions;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.Tournament;

public class UpdateTournamentBannerCommand : IUpdateTournamentBannerCommand
{
	private readonly ILogger<UpdateTournamentBannerCommand> logger;
	private readonly IUploadFileCommand uploadFile;
	private readonly IAccessFileCommand accessFile;
	private readonly IDatabaseTransactionProvider databaseTransactionProvider;
	private readonly IAttachmentRepository attachmentRepository;

	public UpdateTournamentBannerCommand(
		ILogger<UpdateTournamentBannerCommand> logger,
		IUploadFileCommand uploadFile,
		IAccessFileCommand accessFile,
		IDatabaseTransactionProvider databaseTransactionProvider,
		IAttachmentRepository attachmentRepository)
	{
		this.logger = logger;
		this.uploadFile = uploadFile;
		this.accessFile = accessFile;
		this.databaseTransactionProvider = databaseTransactionProvider;
		this.attachmentRepository = attachmentRepository;
	}

	public async Task<Uri> UpdateTournamentBannerAsync(TournamentIdentifier tournamentId, string contentType, Stream content, CancellationToken cancellationToken)
	{
		bool fileUploaded = false;
		try
		{
			await using var transaction = await this.databaseTransactionProvider.BeginAsync();

			const string attachmentName = "banner";
			var attachment = await this.attachmentRepository.GetAttachmentAsync(tournamentId, attachmentName, cancellationToken);

			this.logger.LogInformation("Uploading new banner for tournament ({tournamentId}) of content type '{contentType}'. Tournament had previously a banner: {hadBanner}.", tournamentId, contentType, attachment != null);

			var uploadResult = await this.uploadFile.UploadFileAsync(contentType, content, cancellationToken);
			fileUploaded = true;

			var blob = new ActiveStorageBlob
			{
				Checksum = uploadResult.Checksum,
				ContentType = contentType,
				CreatedAt = DateTime.UtcNow,
				Filename = "na",
				Key = uploadResult.Key,
			};

			this.logger.LogInformation("Setting new banner for tournament ({tournamentId}) in the database.", tournamentId);
			await this.attachmentRepository.UpsertAttachmentAsync(tournamentId, attachmentName, blob, cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			return await this.accessFile.GetFileAccessUriAsync(uploadResult.Key, TimeSpan.FromMinutes(5), cancellationToken);
		}
		catch (Exception ex)
		{
			this.logger.LogError(ex, "Error occurred during tournament banner upload for tournament ({tournamentId}). File uploaded = {fileUploaded}.", tournamentId, fileUploaded);
			throw;
		}
	}
}
