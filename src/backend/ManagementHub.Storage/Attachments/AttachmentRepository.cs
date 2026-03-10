using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.DbAccessors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Attachments;

/// <summary>
/// Repository of attachments on different entities in the database.
/// </summary>
public class AttachmentRepository : IAttachmentRepository
{
	private static readonly Dictionary<Type, string> identifierToRecordTypeMapping = new()
	{
		[typeof(UserIdentifier)] = "User",
		[typeof(NgbIdentifier)] = "NationalGoverningBody",
		[typeof(TournamentIdentifier)] = "Tournament",
		[typeof(TeamIdentifier)] = "Team",
	};

	private readonly ManagementHubDbContext dbContext;
	private readonly ILogger<AttachmentRepository> logger;
	private readonly IDbAccessorProvider dbAccessorProvider;
	private readonly ISystemClock clock;

	public AttachmentRepository(ManagementHubDbContext dbContext, ILogger<AttachmentRepository> logger, IDbAccessorProvider dbAccessorProvider, ISystemClock clock)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.dbAccessorProvider = dbAccessorProvider;
		this.clock = clock;
	}

	public async Task<ActiveStorageAttachment?> GetAttachmentAsync<TId>(TId identifier, string attachmentName, CancellationToken cancellationToken)
	{
		string recordType = GetRecordType<TId>();

		this.logger.LogInformation(0xff45500, "Retrieving attachment '{attachmentName}' for '{recordType}' ({identifier}).", attachmentName, recordType, identifier);

		var recordQueryable = this.dbAccessorProvider.GetDbAccessor<TId>().SelectWithId(identifier).AsNoTracking();
		var attachments = this.dbContext.ActiveStorageAttachments.AsNoTracking().Where(a => a.RecordType == recordType && a.Name == attachmentName);
		return await recordQueryable.Join(attachments, record => record.Id, attachment => attachment.RecordId, (_, attachment) => attachment)
			.Include(a => a.Blob) // IMPORTANT: include the blob in the result
			.SingleOrDefaultAsync(cancellationToken);
	}

	public async Task UpsertAttachmentAsync<TId>(TId identifier, string attachmentName, ActiveStorageBlob blob, CancellationToken cancellationToken)
	{
		string recordType = GetRecordType<TId>();

		this.logger.LogInformation(0xff45501, "Upserting attachment '{attachmentName}' for '{recordType}' ({identifier}).", attachmentName, recordType, identifier);

		this.dbContext.ActiveStorageBlobs.Add(blob);

		var recordQueryable = await this.dbAccessorProvider.GetDbAccessor<TId>().SelectWithId(identifier).SingleAsync(cancellationToken);
		var attachment = await this.dbContext.ActiveStorageAttachments.Where(a => a.RecordType == recordType && a.Name == attachmentName && a.RecordId == recordQueryable.Id)
			.SingleOrDefaultAsync(cancellationToken);

		if (attachment != null)
		{
			attachment.Blob = blob;
			attachment.CreatedAt = this.clock.UtcNow.UtcDateTime;
		}
		else
		{
			attachment = new ActiveStorageAttachment
			{
				Name = attachmentName,
				Blob = blob,
				CreatedAt = this.clock.UtcNow.UtcDateTime,
				RecordId = recordQueryable.Id,
				RecordType = recordType,
			};
			this.dbContext.ActiveStorageAttachments.Add(attachment);
		}

		await this.dbContext.SaveChangesAsync(cancellationToken);
	}

	public Task RemoveBlobAsync(ActiveStorageBlob blob, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0xff45502, "Removing blob with id '{blobId}'.", blob.Id);

		return this.dbContext.ActiveStorageBlobs.Where(b => b.Id == blob.Id).ExecuteDeleteAsync(cancellationToken);
	}

	private static string GetRecordType<TId>()
	{
		if (!identifierToRecordTypeMapping.TryGetValue(typeof(TId), out var recordType))
		{
			throw new InvalidOperationException($"No record type is registered for type {typeof(TId)}.");
		}

		return recordType;
	}
}
