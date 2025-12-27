using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Data;

namespace ManagementHub.Storage.Attachments;

public interface IAttachmentRepository
{
	/// <summary>
	/// Retrieve an attachment with the blob loaded.
	/// </summary>
	/// <typeparam name="TId">Type of the object identifier.</typeparam>
	/// <param name="identifier">Object identifier.</param>
	/// <param name="attachmentName">Attachment name (what kind of attachment it is).</param>
	/// <returns>Attachment object.</returns>
	Task<ActiveStorageAttachment?> GetAttachmentAsync<TId>(TId identifier, string attachmentName, CancellationToken cancellationToken);

	/// <summary>
	/// Upserts an attachement for object <paramref name="identifier"/> for an attachment type <paramref name="attachmentName"/>
	/// with data identified by <paramref name="blob"/>.
	/// The blob is inserted into the database.
	/// </summary>
	/// <typeparam name="TId">Type of the object identifier.</typeparam>
	/// <param name="identifier">Object identifier.</param>
	/// <param name="attachmentName">Attachment name (what kind of attachment it is).</param>
	/// <param name="blob">Blob to be inserted and attached to the object.</param>
	Task UpsertAttachmentAsync<TId>(TId identifier, string attachmentName, ActiveStorageBlob blob, CancellationToken cancellationToken);

	/// <summary>
	/// Remove a blob from the database.
	/// </summary>
	Task RemoveBlobAsync(ActiveStorageBlob blob, CancellationToken cancellationToken);
}
