using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Domain.Language;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Commands;

public class UpdateUserAvatarCommand : IUpdateUserAvatarCommand
{
	private readonly IQueryable<User> users;
	private readonly IQueryable<ActiveStorageAttachment> activeStorageAttachments;
	private readonly IQueryable<ActiveStorageBlob> activeStorageBlobs;
	private readonly ILogger<UpdateUserAvatarCommand> logger;
	private readonly IUploadFileCommand uploadFile;
	private readonly IAccessFileCommand accessFile;
	private readonly IDatabaseTransactionProvider databaseTransactionProvider;

	public UpdateUserAvatarCommand(
		IQueryable<User> users,
		IQueryable<ActiveStorageAttachment> activeStorageAttachments,
		IQueryable<ActiveStorageBlob> activeStorageBlobs,
		ILogger<UpdateUserAvatarCommand> logger,
		IUploadFileCommand uploadFile,
		IAccessFileCommand accessFile,
		IDatabaseTransactionProvider databaseTransactionProvider)
	{
		this.users = users;
		this.activeStorageAttachments = activeStorageAttachments;
		this.activeStorageBlobs = activeStorageBlobs;
		this.logger = logger;
		this.uploadFile = uploadFile;
		this.accessFile = accessFile;
		this.databaseTransactionProvider = databaseTransactionProvider;
	}

	public async Task<Uri> UpdateUserAvatarAsync(UserIdentifier userId, string contentType, Stream avatarStream, CancellationToken cancellationToken)
	{
		await using var transaction = await this.databaseTransactionProvider.BeginAsync();

		var record = await this.users.AsNoTracking().WithIdentifier(userId)
			.Join(this.activeStorageAttachments.Where(a => a.RecordType == "User" && a.Name == "avatar"), u => u.Id, a => a.RecordId, (u, a) => new { User = u, Attachmet = a })
			.Join(this.activeStorageBlobs, x => x.Attachmet.BlobId, b => b.Id, (x, b) => Tuple.Create(x.User.Id, x.Attachmet, b))
			.SingleOrDefaultAsync(cancellationToken);

		this.logger.LogInformation(0, "Uploading new avatar for user ({userId}) of content type '{contentType}'.", userId, contentType);

		var uploadResult = await this.uploadFile.UploadFileAsync(contentType, avatarStream, cancellationToken);
		// TODO remove file if transaction fails

		var blob = new ActiveStorageBlob
		{
			Checksum = uploadResult.Checksum,
			ContentType = contentType,
			CreatedAt = DateTime.UtcNow,
			Filename = "na",
			Key = uploadResult.Key,
		};
		var attachment = new ActiveStorageAttachment
		{
			Blob = blob,
			CreatedAt = DateTime.UtcNow,
			Name = "avatar",
			RecordType = "User",
			RecordId = 0 // TODO
		};
		// TODO: insert?


	}
}
