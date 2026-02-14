using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Attachments;
using ManagementHub.Storage.Database.Transactions;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands.User;

public class UpdateUserAvatarCommand : IUpdateUserAvatarCommand
{
	private readonly ILogger<UpdateUserAvatarCommand> logger;
	private readonly IUploadFileCommand uploadFile;
	private readonly IAccessFileCommand accessFile;
	private readonly IDatabaseTransactionProvider databaseTransactionProvider;
	private readonly IAttachmentRepository attachmentRepository;

	public UpdateUserAvatarCommand(
		ILogger<UpdateUserAvatarCommand> logger,
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

	public async Task<Uri> UpdateUserAvatarAsync(UserIdentifier userId, string contentType, Stream avatarStream, CancellationToken cancellationToken)
	{
		bool fileUploaded = false;
		try
		{
			await using var transaction = await this.databaseTransactionProvider.BeginAsync();

			const string attachmentName = "avatar";
			var attachment = this.attachmentRepository.GetAttachmentAsync(userId, attachmentName, cancellationToken);

			this.logger.LogInformation(0x2d0ef100, "Uploading new avatar for user ({userId}). User had previously an avatar: {hadAvatar}.", userId, attachment != null);

			var uploadResult = await this.uploadFile.UploadFileAsync(contentType, avatarStream, cancellationToken);
			fileUploaded = true;
			// TODO remove file if transaction fails

			var blob = new ActiveStorageBlob
			{
				Checksum = uploadResult.Checksum,
				ContentType = contentType,
				CreatedAt = DateTime.UtcNow,
				Filename = "na",
				Key = uploadResult.Key,
			};

			this.logger.LogInformation(0x2d0ef101, "Setting new avatar for user ({userId}) in the database.", userId);
			await this.attachmentRepository.UpsertAttachmentAsync(userId, attachmentName, blob, cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			// TODO: remove old file in blob storage and remove blob from database

			// TODO: put expiration in settings
			return await this.accessFile.GetFileAccessUriAsync(uploadResult.Key, TimeSpan.FromMinutes(5), cancellationToken);
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x2d0ef102, ex, "Error occurred during user avatar upload for user ({userId}). File uploaded = {fileUploaded}.", userId, fileUploaded);

			// TODO: if uploaded and not committed, remove file.
			throw;
		}
	}

	// TODO: refactor common code into a new method or consider extracting this out to its own class
	public async Task<Uri> UpdateNgbAvatarAsync(NgbIdentifier ngbId, string contentType, Stream avatarStream, CancellationToken cancellationToken)
	{
		bool fileUploaded = false;
		try
		{
			await using var transaction = await this.databaseTransactionProvider.BeginAsync();

			const string attachmentName = "logo";
			var attachment = this.attachmentRepository.GetAttachmentAsync(ngbId, attachmentName, cancellationToken);

			this.logger.LogInformation(0x2d0ef103, "Uploading new avatar for NGB ({ngbId}). NGB had previously an avatar: {hadAvatar}.", ngbId, attachment != null);

			var uploadResult = await this.uploadFile.UploadFileAsync(contentType, avatarStream, cancellationToken);
			fileUploaded = true;
			// TODO remove file if transaction fails

			var blob = new ActiveStorageBlob
			{
				Checksum = uploadResult.Checksum,
				ContentType = contentType,
				CreatedAt = DateTime.UtcNow,
				Filename = "na",
				Key = uploadResult.Key,
			};

			this.logger.LogInformation(0x2d0ef104, "Setting new avatar for NGB ({ngbId}) in the database.", ngbId);
			await this.attachmentRepository.UpsertAttachmentAsync(ngbId, attachmentName, blob, cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			// TODO: remove old file in blob storage and remove blob from database

			// TODO: put expiration in settings
			return await this.accessFile.GetFileAccessUriAsync(uploadResult.Key, TimeSpan.FromMinutes(5), cancellationToken);
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x2d0ef105, ex, "Error occurred during user avatar upload for NGB ({ngbId}). File uploaded = {fileUploaded}.", ngbId, fileUploaded);

			// TODO: if uploaded and not committed, remove file.
			throw;
		}
	}

	public async Task<Uri> UpdateTeamLogoAsync(TeamIdentifier teamId, string contentType, Stream logoStream, CancellationToken cancellationToken)
	{
		bool fileUploaded = false;
		try
		{
			await using var transaction = await this.databaseTransactionProvider.BeginAsync();

			const string attachmentName = "logo";
			var attachment = await this.attachmentRepository.GetAttachmentAsync(teamId, attachmentName, cancellationToken);

			this.logger.LogInformation(0x2d0ef106, "Uploading new logo for team ({teamId}). Team had previously a logo: {hadLogo}.", teamId, attachment != null);

			var uploadResult = await this.uploadFile.UploadFileAsync(contentType, logoStream, cancellationToken);
			fileUploaded = true;
			// TODO remove file if transaction fails

			var blob = new ActiveStorageBlob
			{
				Checksum = uploadResult.Checksum,
				ContentType = contentType,
				CreatedAt = DateTime.UtcNow,
				Filename = "na",
				Key = uploadResult.Key,
			};

			this.logger.LogInformation(0x2d0ef107, "Setting new logo for team ({teamId}) in the database.", teamId);
			await this.attachmentRepository.UpsertAttachmentAsync(teamId, attachmentName, blob, cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			// TODO: remove old file in blob storage and remove blob from database

			// TODO: put expiration in settings
			return await this.accessFile.GetFileAccessUriAsync(uploadResult.Key, TimeSpan.FromMinutes(5), cancellationToken);
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x2d0ef108, ex, "Error occurred during team logo upload for team ({teamId}). File uploaded = {fileUploaded}.", teamId, fileUploaded);

			// TODO: if uploaded and not committed, remove file.
			throw;
		}
	}
}
